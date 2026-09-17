using System;
using System.Collections.Generic;
using System.Linq;
using AmericanOfficialNotes.Model;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 建筑管理 - 放置/拆除、参数读写
    /// 任何形式的参数读取都返回修正后的结果
    /// 公式：实际值 = 基础数值 × 参数修正乘数 + 参数修正加数
    /// </summary>
    public class BuildingController
    {
        private readonly GameSaveData _gameData;
        private readonly GameManager _gameManager;

        public BuildingController(GameSaveData gameData, GameManager gameManager)
        {
            _gameData = gameData;
            _gameManager = gameManager;
        }

        /// <summary>放置建筑</summary>
        public BuildingData PlaceBuilding(int tileX, int tileY, BuildingType type)
        {
            var tile = _gameManager.TileController.GetTile(tileX, tileY);
            if (tile == null || tile.HasBuilding) return null;

            var building = new BuildingData
            {
                Type = type,
                Category = tile.Category
            };
            InitializeBuildingParameters(building, tile.Level);
            tile.Building = building;
            tile.RadiationRange = GetRadiationRange(type);

            // 注册此建筑的修正效果（如果有）
            ApplyModifierEffects(tile);

            // 检查现有来源建筑是否对此新建筑有修正
            RegisterIncomingModifiers(tile);

            _gameManager.InvokeBuildingPlaced(tile);
            return building;
        }

        /// <summary>拆除建筑</summary>
        public bool RemoveBuilding(int tileX, int tileY)
        {
            var tile = _gameManager.TileController.GetTile(tileX, tileY);
            if (tile == null || !tile.HasBuilding) return false;

            // 移除此建筑发出的修正
            _gameManager.ModifierController.RemoveModifiersFromSource(tileX, tileY);
            // 移除作用于此建筑的修正
            _gameManager.ModifierController.RemoveModifiersForTarget(tileX, tileY);

            tile.Building = null;
            tile.RadiationRange = 0f;

            _gameManager.InvokeBuildingRemoved(tile);
            return true;
        }

        /// <summary>
        /// 获取参数（修正后值）- 任何读取都返回修正后的结果
        /// 公式：实际值 = 基础数值 × 乘数 + 加数
        /// </summary>
        public float GetParameter(int tileX, int tileY, BuildingParameterType paramType)
        {
            var tile = _gameManager.TileController.GetTile(tileX, tileY);
            if (tile == null || !tile.HasBuilding) return 0f;

            float baseValue = tile.Building.GetBaseParameter(paramType);
            float baseAdditive = _gameManager.ModifierController.GetTotalBaseAdditive(tileX, tileY, paramType);
            float multiplier = _gameManager.ModifierController.GetTotalMultiplier(tileX, tileY, paramType);
            float additive = _gameManager.ModifierController.GetTotalModifier(tileX, tileY, paramType);
            return (baseValue + baseAdditive) * multiplier + additive;
        }

        /// <summary>获取基础参数值（未经修正）</summary>
        public float GetBaseParameter(int tileX, int tileY, BuildingParameterType paramType)
        {
            var tile = _gameManager.TileController.GetTile(tileX, tileY);
            if (tile == null || !tile.HasBuilding) return 0f;
            return tile.Building.GetBaseParameter(paramType);
        }

        /// <summary>
        /// 获取参数详情（基础值 + 修正列表）- 供UI显示运算过程
        /// </summary>
        public (float baseValue, List<ParameterModifier> modifiers, float finalValue) GetParameterDetail(
            int tileX, int tileY, BuildingParameterType paramType)
        {
            var tile = _gameManager.TileController.GetTile(tileX, tileY);
            if (tile == null || !tile.HasBuilding)
                return (0f, new List<ParameterModifier>(), 0f);

            float baseValue = tile.Building.GetBaseParameter(paramType);
            var modifiers = _gameManager.ModifierController.GetModifiersFor(tileX, tileY, paramType);
            float baseAdditive = _gameManager.ModifierController.GetTotalBaseAdditive(tileX, tileY, paramType);
            float multiplier = _gameManager.ModifierController.GetTotalMultiplier(tileX, tileY, paramType);
            float additive = _gameManager.ModifierController.GetTotalModifier(tileX, tileY, paramType);
            float finalValue = (baseValue + baseAdditive) * multiplier + additive;
            return (baseValue, modifiers, finalValue);
        }

        /// <summary>
        /// 重新计算所有修正（每日结算后调用）
        /// </summary>
        public void RecalculateAllModifiers()
        {
            _gameManager.ModifierController.ClearAll();
            foreach (var tile in _gameData.Tiles)
            {
                if (tile.HasBuilding && tile.Building.HasParameter(BuildingParameterType.ModifierBonus))
                {
                    ApplyModifierEffects(tile);
                }
            }
        }

        /// <summary>
        /// 应用地块建筑的修正效果 - 向辐射范围内建筑注册修正
        /// </summary>
        private void ApplyModifierEffects(TileData sourceTile)
        {
            if (!sourceTile.HasBuilding) return;
            if (!sourceTile.Building.HasParameter(BuildingParameterType.ModifierBonus)) return;

            float bonus = sourceTile.Building.GetBaseParameter(BuildingParameterType.ModifierBonus);
            var targets = _gameManager.TileController.GetTilesInRadiationRange(
                sourceTile.GridX, sourceTile.GridY, sourceTile.RadiationRange);

            foreach (var target in targets)
            {
                if (!target.HasBuilding) continue;
                // 社会服务修正目标建筑的所有参数
                foreach (var paramType in target.Building.GetParameterTypes())
                {
                    var modifier = new ParameterModifier
                    {
                        SourceTileX = sourceTile.GridX,
                        SourceTileY = sourceTile.GridY,
                        TargetTileX = target.GridX,
                        TargetTileY = target.GridY,
                        SourceBuildingType = sourceTile.Building.Type,
                        TargetParameter = paramType,
                        ModifierValue = bonus
                    };
                    _gameManager.ModifierController.RegisterModifier(modifier);
                }
            }
        }

        /// <summary>
        /// 注册来自现有来源建筑的修正（新建筑放置时检查）
        /// </summary>
        private void RegisterIncomingModifiers(TileData targetTile)
        {
            foreach (var source in _gameData.Tiles)
            {
                if (source == targetTile || !source.HasBuilding) continue;
                if (!source.Building.HasParameter(BuildingParameterType.ModifierBonus)) continue;
                if (!source.IsInRadiationRange(targetTile)) continue;

                float bonus = source.Building.GetBaseParameter(BuildingParameterType.ModifierBonus);
                foreach (var paramType in targetTile.Building.GetParameterTypes())
                {
                    var modifier = new ParameterModifier
                    {
                        SourceTileX = source.GridX,
                        SourceTileY = source.GridY,
                        TargetTileX = targetTile.GridX,
                        TargetTileY = targetTile.GridY,
                        SourceBuildingType = source.Building.Type,
                        TargetParameter = paramType,
                        ModifierValue = bonus
                    };
                    _gameManager.ModifierController.RegisterModifier(modifier);
                }
            }
        }

        /// <summary>获取建筑辐射范围（由建筑类型决定）</summary>
        private float GetRadiationRange(BuildingType type)
        {
            // 社会服务建筑有辐射范围，其他建筑为0
            switch (type)
            {
                case BuildingType.Hospital:
                case BuildingType.PoliceStation:
                case BuildingType.School:
                    return 5f; // 占位值，后续由数据配置
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 初始化建筑基础参数（占位 - 后续由数据配置替换）
        /// 地块等级越高基础数值越高
        /// </summary>
        private void InitializeBuildingParameters(BuildingData building, int tileLevel)
        {
            float levelMult = 1f + (tileLevel - 1) * 0.2f;

            switch (building.Type)
            {
                // === 居民区 ===
                case BuildingType.RuralResidential:
                    building.SetBaseParameter(BuildingParameterType.PopulationCap, 10 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.PerCapitaConsumption, 30f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.Health, 80f);
                    building.SetBaseParameter(BuildingParameterType.Mood, 70f);
                    break;
                case BuildingType.TownshipResidential:
                    building.SetBaseParameter(BuildingParameterType.PopulationCap, 50 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.PerCapitaConsumption, 50f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.Health, 75f);
                    building.SetBaseParameter(BuildingParameterType.Mood, 65f);
                    break;
                case BuildingType.HighDensityResidential:
                    building.SetBaseParameter(BuildingParameterType.PopulationCap, 200 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.PerCapitaConsumption, 80f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.Health, 60f);
                    building.SetBaseParameter(BuildingParameterType.Mood, 50f);
                    break;

                // === 商业区 ===
                case BuildingType.SmallSupermarket:
                case BuildingType.LargeSupermarket:
                case BuildingType.IndividualDining:
                case BuildingType.ChainDining:
                case BuildingType.BathMassage:
                case BuildingType.BoardCards:
                case BuildingType.RepairShop:
                case BuildingType.SportsVenue:
                case BuildingType.InternetCafe:
                case BuildingType.ExpressDelivery:
                    building.SetBaseParameter(BuildingParameterType.JobCount, 10 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.AverageWage, 100f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.ProductInventory, 100 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.AveragePrice, 20f);
                    building.SetBaseParameter(BuildingParameterType.MaxSales, 50 * levelMult);
                    break;

                // === 农业畜牧 ===
                case BuildingType.NormalFarmland:
                case BuildingType.QualityFarmland:
                case BuildingType.ModernFarmland:
                    building.SetBaseParameter(BuildingParameterType.MaxCropYield, 100 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.JobCount, 5 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.AverageWage, 80f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.ProductionEfficiency, 1f);
                    break;
                case BuildingType.NormalShed:
                case BuildingType.QualityShed:
                case BuildingType.ModernShed:
                    building.SetBaseParameter(BuildingParameterType.MaxLivestockYield, 50 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.JobCount, 5 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.AverageWage, 80f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.ProductionEfficiency, 1f);
                    break;

                // === 工业区 ===
                case BuildingType.SmallAgriculturalIndustry:
                case BuildingType.IntensiveIndustry:
                case BuildingType.ModernIndustry:
                    building.SetBaseParameter(BuildingParameterType.JobCount, 20 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.AverageWage, 120f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.MaterialDemand, 50 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.MaxProductYield, 80 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.ProductionEfficiency, 1f);
                    building.SetBaseParameter(BuildingParameterType.BacklogInventory, 0);
                    building.ProductName = "通用产品";
                    break;

                // === 社会服务 ===
                case BuildingType.Hospital:
                case BuildingType.PoliceStation:
                case BuildingType.School:
                    building.SetBaseParameter(BuildingParameterType.JobCount, 10 * levelMult);
                    building.SetBaseParameter(BuildingParameterType.AverageWage, 150f * levelMult);
                    building.SetBaseParameter(BuildingParameterType.ModifierBonus, 0.1f * levelMult);
                    break;
            }
        }
    }
}