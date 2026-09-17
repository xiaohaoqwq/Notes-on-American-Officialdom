using System.Collections.Generic;
using System.Linq;
using AmericanOfficialNotes.Model;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 参数修正管理 - 注册/移除/叠加/实时重算
    /// 修正可以叠加，几乎可以无限叠加
    /// 所有参数读取都返回修正后的结果
    /// </summary>
    public class ModifierController
    {
        private readonly GameManager _gameManager;

        // 按目标地块索引：TileKey(targetX, targetY) → 修正列表
        private readonly Dictionary<string, List<ParameterModifier>> _modifiersByTarget = new Dictionary<string, List<ParameterModifier>>();

        // 按来源地块索引：TileKey(sourceX, sourceY) → 修正列表
        private readonly Dictionary<string, List<ParameterModifier>> _modifiersBySource = new Dictionary<string, List<ParameterModifier>>();

        private static string TileKey(int x, int y) => $"{x},{y}";

        public ModifierController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        /// <summary>注册参数修正（实时传输给目标建筑）</summary>
        public void RegisterModifier(ParameterModifier modifier)
        {
            string targetKey = TileKey(modifier.TargetTileX, modifier.TargetTileY);
            string sourceKey = TileKey(modifier.SourceTileX, modifier.SourceTileY);

            if (!_modifiersByTarget.ContainsKey(targetKey))
                _modifiersByTarget[targetKey] = new List<ParameterModifier>();
            _modifiersByTarget[targetKey].Add(modifier);

            if (!_modifiersBySource.ContainsKey(sourceKey))
                _modifiersBySource[sourceKey] = new List<ParameterModifier>();
            _modifiersBySource[sourceKey].Add(modifier);
        }

        /// <summary>移除某来源地块的所有修正（建筑拆除时调用）</summary>
        public void RemoveModifiersFromSource(int sourceTileX, int sourceTileY)
        {
            string sourceKey = TileKey(sourceTileX, sourceTileY);
            if (!_modifiersBySource.TryGetValue(sourceKey, out var sourceList)) return;

            var toRemove = sourceList.ToList();
            foreach (var modifier in toRemove)
            {
                string targetKey = TileKey(modifier.TargetTileX, modifier.TargetTileY);
                if (_modifiersByTarget.TryGetValue(targetKey, out var targetList))
                {
                    targetList.Remove(modifier);
                    if (targetList.Count == 0)
                        _modifiersByTarget.Remove(targetKey);
                }
            }
            _modifiersBySource.Remove(sourceKey);
        }

        /// <summary>移除某目标地块的所有修正（目标建筑拆除时调用）</summary>
        public void RemoveModifiersForTarget(int targetTileX, int targetTileY)
        {
            string targetKey = TileKey(targetTileX, targetTileY);
            if (!_modifiersByTarget.TryGetValue(targetKey, out var targetList)) return;

            var toRemove = targetList.ToList();
            foreach (var modifier in toRemove)
            {
                string sourceKey = TileKey(modifier.SourceTileX, modifier.SourceTileY);
                if (_modifiersBySource.TryGetValue(sourceKey, out var sourceList))
                {
                    sourceList.Remove(modifier);
                    if (sourceList.Count == 0)
                        _modifiersBySource.Remove(sourceKey);
                }
            }
            _modifiersByTarget.Remove(targetKey);
        }

        /// <summary>获取影响某地块某参数的所有修正（用于UI显示运算过程）</summary>
        public List<ParameterModifier> GetModifiersFor(int tileX, int tileY, BuildingParameterType paramType)
        {
            string key = TileKey(tileX, tileY);
            if (!_modifiersByTarget.TryGetValue(key, out var list)) return new List<ParameterModifier>();
            return list.Where(m => m.TargetParameter == paramType).ToList();
        }

        /// <summary>获取某地块某参数的修正总和（加数部分，可无限叠加）</summary>
        public float GetTotalModifier(int tileX, int tileY, BuildingParameterType paramType)
        {
            return GetModifiersFor(tileX, tileY, paramType).Sum(m => m.ModifierValue);
        }

        /// <summary>获取某地块某参数的修正乘数总积（默认1，叠加为各乘数相乘）</summary>
        public float GetTotalMultiplier(int tileX, int tileY, BuildingParameterType paramType)
        {
            var mods = GetModifiersFor(tileX, tileY, paramType);
            if (mods.Count == 0) return 1f;
            float result = 1f;
            foreach (var m in mods)
                result *= m.ModifierMultiplier;
            return result;
        }

        /// <summary>获取某地块某参数的原数值加数总和（默认0）</summary>
        public float GetTotalBaseAdditive(int tileX, int tileY, BuildingParameterType paramType)
        {
            return GetModifiersFor(tileX, tileY, paramType).Sum(m => m.BaseAdditive);
        }

        /// <summary>获取某地块所有修正</summary>
        public List<ParameterModifier> GetAllModifiersFor(int tileX, int tileY)
        {
            string key = TileKey(tileX, tileY);
            if (!_modifiersByTarget.TryGetValue(key, out var list)) return new List<ParameterModifier>();
            return list.ToList();
        }

        /// <summary>清除所有修正（结算前重置）</summary>
        public void ClearAll()
        {
            _modifiersByTarget.Clear();
            _modifiersBySource.Clear();
        }
    }
}