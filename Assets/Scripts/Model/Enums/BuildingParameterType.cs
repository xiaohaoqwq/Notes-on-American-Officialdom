namespace AmericanOfficialNotes.Model.Enums
{
    /// <summary>
    /// 建筑参数类型 - 涵盖所有建筑类别的参数
    /// </summary>
    public enum BuildingParameterType
    {
        // === 居民区参数 ===
        CurrentPopulation,      // 现有人口数量（整数）
        PopulationCap,          // 人口数量上限（整数）
        OccupancyRate,          // 入住率
        PerCapitaConsumption,   // 人均日消费实力（精确到后二位小数）
        TotalConsumption,       // 总体消费实力
        Health,                 // 健康
        Mood,                   // 心情

        // === 商业区参数 ===
        JobCount,               // 职位数量（整数）
        EmployeeCount,          // 在职员工数（整数）
        ProductInventory,       // 商品库存数（整数）
        AveragePrice,           // 平均商品价格
        MaxSales,               // 最大销量
        AverageWage,            // 平均工资

        // === 农业畜牧区参数 ===
        MaxCropYield,           // 最大农作物原料产量
        MaxLivestockYield,      // 最大肉蛋奶原料产量
        // JobCount, EmployeeCount, AverageWage, ProductionEfficiency 复用

        // === 工业区参数 ===
        MaterialDemand,         // 原料需求量
        MaxProductYield,        // 最大产品产量
        // JobCount, EmployeeCount, AverageWage, ProductionEfficiency 复用

        // === 社会服务参数 ===
        ModifierBonus,          // 给予其他所有建筑参数的修正量
        // JobCount, EmployeeCount, AverageWage 复用

        // === 通用参数 ===
        ProductionEfficiency,   // 生产效率（农业、工业共用）

        // === 工业区新增 ===
        BacklogInventory,       // 积压库存

        // === 地块通用劳动力参数 ===
        LaborConsumption,       // 单人劳动力消耗量
        LaborRecovery           // 单人劳动力回复量
    }
}