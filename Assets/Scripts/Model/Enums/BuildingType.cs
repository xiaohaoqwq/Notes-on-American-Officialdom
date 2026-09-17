namespace AmericanOfficialNotes.Model.Enums
{
    /// <summary>
    /// 建筑类型 - 每个地块分类下有不同的具体建筑
    /// </summary>
    public enum BuildingType
    {
        // === 居民区 ===
        RuralResidential,        // 农家居民区
        TownshipResidential,     // 乡镇居民区
        HighDensityResidential,  // 高密度居民区

        // === 商业区 ===
        SmallSupermarket,        // 小型商超
        LargeSupermarket,        // 大型商超
        IndividualDining,        // 个体餐饮
        ChainDining,             // 连锁餐饮
        BathMassage,             // 洗浴按摩
        BoardCards,              // 棋牌
        RepairShop,              // 维修店
        SportsVenue,             // 体育场馆
        InternetCafe,            // 网吧电竞
        ExpressDelivery,         // 快递跑腿

        // === 农业畜牧 ===
        NormalFarmland,          // 普通农田
        QualityFarmland,         // 优良农田
        ModernFarmland,          // 现代化农田
        NormalShed,              // 普通棚舍
        QualityShed,             // 优良棚舍
        ModernShed,              // 现代化棚舍

        // === 工业区 ===
        SmallAgriculturalIndustry, // 小农工业
        IntensiveIndustry,         // 密集型工业
        ModernIndustry,            // 现代化工业

        // === 社会服务 ===
        Hospital,                // 医院
        PoliceStation,           // 警局
        School                   // 学校
    }
}