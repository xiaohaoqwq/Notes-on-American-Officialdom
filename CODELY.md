# 买官录（美国官场笔记）

## 项目概述

Unity 2D 城建模拟经营游戏。玩家在美国购买土地、建设都市，可选择不同发展路线（人民至上、官商勾结、土地财政）。核心机制包括地块网格系统、建筑参数修正、人口劳动力结算、每日结算系统。

- **引擎**: Tuanjie 1.6.13（Unity 兼容引擎），URP 2D
- **架构**: MVC（Model-View-Controller）
- **命名空间**: `AmericanOfficialNotes.Model` / `.Controller` / `.View`

## MVC 架构

```
Assets/Scripts/
├── Model/                        # 数据层（纯C#，不依赖Unity）
│   ├── Enums/
│   │   ├── TileCategory.cs       # 地块分类：居民区/商业区/农业畜牧/工业区/社会服务
│   │   ├── BuildingType.cs       # 建筑类型（21种）
│   │   └── BuildingParameterType.cs # 建筑参数类型枚举
│   ├── TileData.cs               # 地块数据（坐标、分类、等级、辐射范围、劳动力参数）
│   ├── BuildingData.cs           # 建筑数据（类型、参数字典）
│   ├── PopulationData.cs         # 人口数据（UUID、姓名、性别、劳动力、金钱、存款）
│   ├── ParameterModifier.cs     # 参数修正（来源→目标、乘数+加数）
│   ├── NameLibrary.cs            # 中文姓名库（姓、男名、女名分开）
│   └── GameSaveData.cs           # 全局存档（天数、资金、地块、人口、姓名库）
│
├── Controller/                   # 逻辑层
│   ├── GameManager.cs            # MonoBehaviour单例，持有所有Controller，事件中心
│   ├── TileController.cs         # 地块创建/拆除/查询/距离计算
│   ├── BuildingController.cs     # 建筑放置/拆除，参数读取返回修正后值
│   ├── SettlementController.cs   # 每日结算：人口流入→劳动力→人口效果→修正重算
│   ├── ModifierController.cs     # 修正注册/移除/叠加/查询
│   ├── PopulationController.cs   # 人口流入/劳动力结算/在职率/名字生成
│   ├── PolicyController.cs       # 政策系统（框架）
│   └── TimeController.cs        # 日期推进，触发结算
│
└── View/                         # 表现层（MonoBehaviour）
    ├── TileView.cs               # 地块渲染、边框高亮
    ├── GridRenderer.cs           # 网格创建与管理
    ├── BuildingView.cs           # 建筑视觉对象
    ├── TileInfoPanel.cs          # 地块信息面板（CanvasGroup控制显隐）
    ├── BuildingInfoPanel.cs      # 建筑参数面板（CanvasGroup控制显隐）
    ├── TopBarUI.cs               # 顶部状态栏（资金/日期/人口）
    ├── ConstructionInformationUI.cs # 建筑分类列表（轮询枚举，按键高亮）
    └── InputHandler.cs           # 键鼠输入→Controller（含滚轮缩放、中键平移、鼠标高亮）
```

## 核心系统

### 参数修正公式

```
实际值 = 基础数值 × 参数修正乘数 + 参数修正加数
```

- 乘数默认为 1，各修正乘数相乘叠加
- 加数默认为 0，各修正加数相加叠加
- 所有参数读取都返回修正后的结果，非原始值

### 生产效率（Sigmoid）

```csharp
static float Sigmoid(float x) => 1f / (1f + MathF.Pow(2.72f, 3.5f - 7f * x));
// x = 员工在职率 = 工作人数 / 职位数量（0-1，不超过1）
```

### 每日结算顺序

1. 人口流入（有用工需求时自动新增人口）
2. 每小时劳动力结算（24小时循环：工作消耗、居住回复）
3. 人口效果结算（更新建筑参数）
4. 参数修正重算
5. 结果生效（触发事件通知 View 刷新）

## 场景结构

```
SampleScene
├── Main Camera (正交, orthographicSize=6)
├── Global Light 2D
├── GameManager (GameManager.cs)
├── GridRoot (GridRenderer.cs)
├── UICanvas (Canvas + CanvasScaler + GraphicRaycaster)
│   ├── TopBar (TopBarUI.cs)
│   │   ├── FundsText
│   │   ├── DayText
│   │   └── PopText
│   ├── TileInfoPanel (TileInfoPanel.cs + CanvasGroup)
│   │   ├── TitleText
│   │   └── InfoText
│   ├── BuildingInfoPanel (BuildingInfoPanel.cs + CanvasGroup)
│   │   ├── TitleText
│   │   └── InfoText
│   └── ConstructionInformation (ConstructionInformationUI.cs)
│       ├── Title
│       └── ListContainer
└── InputHandler (InputHandler.cs)
```

## 开发约定

- **Model 层不依赖 Unity**：纯 C# class，可独立测试
- **GameManager 是唯一数据写入入口**：策划案要求参数只能通过 GameManager 更改
- **事件驱动**：Controller 通过 GameManager 的 `Invoke*` 方法触发事件，View 层订阅
- **UI 用 CanvasGroup 控制显隐**：不用 `SetActive(false)` 自身（会导致 `FindObjectOfType` 找不到）
- **地块坐标对齐**：tile 世界坐标 = `(GridX + 0.5, GridY + 0.5)`，与 `Floor(worldPos)` 网格判定一致
- **人口 ID 用 UUID**：`Guid.NewGuid().ToString()`，不与姓名绑定
- **姓名从 NameLibrary 生成**：姓+名组合，男女分开存储

## 测试操作（Play Mode）

| 按键 | 功能 |
|------|------|
| `T` | 生成 5×5 测试网格 |
| `1-5` | 切换建筑分类（高亮 UI 对应项） |
| `B` | 鼠标位置放置建筑（无地块时自动创建） |
| `R` | 鼠标位置拆除地块 |
| `Space` | 推进一天（触发结算） |
| 鼠标左键 | 点击地块显示信息面板 |
| 鼠标滚轮 | 缩放画布（3-15） |
| 鼠标中键拖拽 | 平移画布 |
