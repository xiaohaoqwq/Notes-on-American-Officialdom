using System;
using System.Linq;
using AmericanOfficialNotes.Model;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 结算系统 - 游戏内每小时结算
    /// 公式：最终值 = (原数值 + 原数值加数) × 修正乘数 + 修正加数
    /// </summary>
    public class SettlementController
    {
        private readonly GameSaveData _gameData;
        private readonly GameManager _gameManager;

        // 农业测试参数
        private const float AgriLaborConsumption = 10f;
        private const int AgriJobCount = 3;
        // 居民区测试参数
        private const float ResLaborRecovery = 12f;

        public SettlementController(GameSaveData gameData, GameManager gameManager)
        {
            _gameData = gameData;
            _gameManager = gameManager;
        }

        /// <summary>基础生产效率 Sigmoid 公式</summary>
        public static float Sigmoid(float x) => 1f / (1f + MathF.Pow(2.72f, 3.5f - 7f * x));

        /// <summary>执行每日结算（由TimeController每日触发）</summary>
        public void Settle()
        {
            // ① 人口流入（有用工需求时新增人口）
            _gameManager.PopulationController.ProcessPopulationInflow();

            // ② 参数修正重算
            _gameManager.BuildingController.RecalculateAllModifiers();

            // ③ 结果生效
            _gameManager.TriggerParametersChanged();
        }

        /// <summary>每小时结算（由TimeController每小时触发）</summary>
        public void SettleHour()
        {
            // ① 劳动力结算（工作消耗 / 居住回复）
            _gameManager.PopulationController.ProcessHourlyLabor();

            // ② 每小时建筑效果结算
            ProcessHourlyBuildingEffects();

            // ③ 结果生效
            _gameManager.TriggerParametersChanged();
        }

        /// <summary>每小时建筑效果结算</summary>
        private void ProcessHourlyBuildingEffects()
        {
            foreach (var tile in _gameData.Tiles)
            {
                if (!tile.HasBuilding) continue;

                switch (tile.Building.Category)
                {
                    case TileCategory.Residential:
                        SettleResidentialHourly(tile);
                        break;
                    case TileCategory.Agricultural:
                        SettleAgriculturalHourly(tile);
                        break;
                    case TileCategory.Industrial:
                        SettleIndustrialHourly(tile);
                        break;
                    case TileCategory.Commercial:
                    case TileCategory.SocialService:
                        // 商业区、社会服务区待策划补充
                        break;
                }
            }
        }

        /// <summary>居民区每小时结算 - 更新人口参数（不受在职率/效率影响）</summary>
        private void SettleResidentialHourly(TileData tile)
        {
            var residents = _gameManager.PopulationController.GetResidentsAt(tile.GridX, tile.GridY);
            int count = residents.Count;

            tile.Building.SetBaseParameter(BuildingParameterType.CurrentPopulation, count);
            int cap = (int)tile.Building.GetBaseParameter(BuildingParameterType.PopulationCap);
            tile.Building.SetBaseParameter(BuildingParameterType.OccupancyRate,
                cap > 0 ? (float)count / cap : 0f);

            if (count > 0)
            {
                float avgHealth = residents.Average(p => p.Health);
                float avgMood = residents.Average(p => p.Mood);
                tile.Building.SetBaseParameter(BuildingParameterType.Health, avgHealth);
                tile.Building.SetBaseParameter(BuildingParameterType.Mood, avgMood);
            }
        }

        /// <summary>农业畜牧区每小时结算</summary>
        private void SettleAgriculturalHourly(TileData tile)
        {
            var workers = _gameManager.PopulationController.GetWorkersAt(tile.GridX, tile.GridY);
            tile.Building.SetBaseParameter(BuildingParameterType.EmployeeCount, workers.Count);

            // 在职率
            float empRate = _gameManager.PopulationController.GetEmploymentRate(tile.GridX, tile.GridY);
            float eff = Sigmoid(empRate);
            tile.Building.SetBaseParameter(BuildingParameterType.ProductionEfficiency, eff);

            // 基础每小时产量 = 该小时建筑内接收到的所有员工的劳动力消耗总量 × 生产效率
            float totalLaborConsumed = 0f;
            foreach (var w in workers)
            {
                // 实际消耗 = min(建筑需求, 人口已有劳动力)
                float consumed = Math.Min(AgriLaborConsumption, w.Labor);
                totalLaborConsumed += consumed;
            }
            float baseYield = totalLaborConsumed * eff;

            // 无人也有少量产出（荒地也能长作物）
            if (workers.Count == 0)
                baseYield = 0.5f * eff;

            // 代入修正公式得出最终产量
            float finalYield = _gameManager.BuildingController.GetParameter(
                tile.GridX, tile.GridY, BuildingParameterType.MaxCropYield);
            // 用基础产量更新原数值，修正后值由GetParameter读取
            tile.Building.SetBaseParameter(BuildingParameterType.MaxCropYield, baseYield);

            // 人口收入 = 每小时最终产量 × 该人口消耗的劳动力 / 总劳动力消耗
            if (totalLaborConsumed > 0)
            {
                foreach (var w in workers)
                {
                    float consumed = Math.Min(AgriLaborConsumption, w.Labor);
                    float income = finalYield * consumed / totalLaborConsumed;
                    w.Money += income;
                }
            }
        }

        /// <summary>工业区每小时结算</summary>
        private void SettleIndustrialHourly(TileData tile)
        {
            var workers = _gameManager.PopulationController.GetWorkersAt(tile.GridX, tile.GridY);
            tile.Building.SetBaseParameter(BuildingParameterType.EmployeeCount, workers.Count);

            float empRate = _gameManager.PopulationController.GetEmploymentRate(tile.GridX, tile.GridY);
            tile.Building.SetBaseParameter(BuildingParameterType.ProductionEfficiency, Sigmoid(empRate));
        }
    }
}
