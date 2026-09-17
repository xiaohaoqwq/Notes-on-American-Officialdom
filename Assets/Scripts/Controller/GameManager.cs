using System;
using UnityEngine;
using AmericanOfficialNotes.Model;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 游戏主管理器 - MonoBehaviour单例
    /// 持有所有Controller和GameData，是唯一的数据写入入口
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // === Model ===
        public GameSaveData GameData { get; private set; }

        // === Controllers ===
        public TileController TileController { get; private set; }
        public BuildingController BuildingController { get; private set; }
        public SettlementController SettlementController { get; private set; }
        public ModifierController ModifierController { get; private set; }
        public PopulationController PopulationController { get; private set; }
        public PolicyController PolicyController { get; private set; }
        public TimeController TimeController { get; private set; }

        // === Events (供View层订阅) ===
        public event Action<TileData> OnTileCreated;
        public event Action<TileData> OnTileRemoved;
        public event Action<TileData> OnBuildingPlaced;
        public event Action<TileData> OnBuildingRemoved;
        public event Action<int> OnDayAdvanced;
        public event Action<decimal> OnFundsChanged;
        public event Action OnParametersChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeGame();
        }

        /// <summary>初始化新游戏</summary>
        public void InitializeGame()
        {
            GameData = new GameSaveData
            {
                CurrentDay = 1,
                Funds = 1_000_000m,  // 百万现金起手
                RouteChoice = string.Empty
            };

            // 创建所有Controller（GameManager作为枢纽传递自身引用）
            ModifierController = new ModifierController(this);
            TileController = new TileController(GameData, this);
            BuildingController = new BuildingController(GameData, this);
            SettlementController = new SettlementController(GameData, this);
            PopulationController = new PopulationController(GameData, this);
            PolicyController = new PolicyController(this);
            TimeController = new TimeController(GameData, this);

            // 初始化建筑分类信息UI
            var constructionUI = FindFirstObjectByType<View.ConstructionInformationUI>();
            if (constructionUI != null)
                constructionUI.Init();
        }

        // === 资金管理 ===
        public decimal GetFunds() => GameData.Funds;

        public bool SpendFunds(decimal amount)
        {
            if (GameData.Funds < amount) return false;
            GameData.Funds -= amount;
            OnFundsChanged?.Invoke(GameData.Funds);
            return true;
        }

        public void AddFunds(decimal amount)
        {
            GameData.Funds += amount;
            OnFundsChanged?.Invoke(GameData.Funds);
        }

        // === 路线选择 ===
        public void SetRoute(string route)
        {
            GameData.RouteChoice = route;
        }

        public string GetRoute() => GameData.RouteChoice;

        /// <summary>触发参数变更通知（供SettlementController调用）</summary>
        public void TriggerParametersChanged()
        {
            OnParametersChanged?.Invoke();
        }

        // === 事件触发方法（供Controller调用，绕过event只能在声明类内调用的限制） ===
        public void InvokeTileCreated(TileData tile) => OnTileCreated?.Invoke(tile);
        public void InvokeTileRemoved(TileData tile) => OnTileRemoved?.Invoke(tile);
        public void InvokeBuildingPlaced(TileData tile) => OnBuildingPlaced?.Invoke(tile);
        public void InvokeBuildingRemoved(TileData tile) => OnBuildingRemoved?.Invoke(tile);
        public void InvokeDayAdvanced(int day) => OnDayAdvanced?.Invoke(day);
    }
}