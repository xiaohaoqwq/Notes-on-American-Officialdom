using System;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Model
{
    /// <summary>
    /// 参数修正 - 某建筑对辐射范围内建筑参数的修正
    /// 修正可以叠加，几乎可以无限叠加
    /// </summary>
    [Serializable]
    public class ParameterModifier
    {
        /// <summary>修正来源建筑所在地块X坐标</summary>
        public int SourceTileX { get; set; }

        /// <summary>修正来源建筑所在地块Y坐标</summary>
        public int SourceTileY { get; set; }

        /// <summary>被修正建筑所在地块X坐标</summary>
        public int TargetTileX { get; set; }

        /// <summary>被修正建筑所在地块Y坐标</summary>
        public int TargetTileY { get; set; }

        /// <summary>修正来源建筑类型（用于UI显示来源）</summary>
        public BuildingType SourceBuildingType { get; set; }

        /// <summary>修正的目标参数类型</summary>
        public BuildingParameterType TargetParameter { get; set; }

        /// <summary>参数修正乘数（默认1，即不改变）</summary>
        public float ModifierMultiplier { get; set; } = 1f;

        /// <summary>原数值加数（加到原数值上，默认0）</summary>
        public float BaseAdditive { get; set; } = 0f;

        /// <summary>参数修正加数（正为加成，负为减损）</summary>
        public float ModifierValue { get; set; }

        /// <summary>修正来源标识（建筑/政策/大商人等）</summary>
        public string SourceTag { get; set; }
    }
}