using System;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Model
{
    /// <summary>
    /// 地块数据 - 纯C#数据结构，不依赖Unity
    /// 本质是网格（平面直角坐标系）
    /// </summary>
    [Serializable]
    public class TileData
    {
        /// <summary>网格X坐标</summary>
        public int GridX { get; set; }

        /// <summary>网格Y坐标</summary>
        public int GridY { get; set; }

        /// <summary>地块分类（一经设置不可更改，除非手动拆除）</summary>
        public TileCategory Category { get; set; }

        /// <summary>地块等级 - 决定在此建造的建筑的基础数值</summary>
        public int Level { get; set; }

        /// <summary>地块建造时间（游戏内天数）- 由等级决定</summary>
        public int BuildTimeDays { get; set; }

        /// <summary>
        /// 辐射范围 - 以地块间距离为准，某值以内的所有地块均在辐射范围
        /// 当地块没有建筑时为0
        /// </summary>
        public float RadiationRange { get; set; }

        /// <summary>单人劳动力消耗量（建筑决定，受参数修正影响）</summary>
        public float LaborConsumption { get; set; }

        /// <summary>单人劳动力回复量（建筑决定，受参数修正影响）</summary>
        public float LaborRecovery { get; set; }

        /// <summary>当前建筑（null表示未建造）</summary>
        public BuildingData Building { get; set; }

        /// <summary>是否已建造建筑</summary>
        public bool HasBuilding => Building != null;

        /// <summary>几何中心X坐标（用于距离计算）</summary>
        public float CenterX => GridX + 0.5f;

        /// <summary>几何中心Y坐标（用于距离计算）</summary>
        public float CenterY => GridY + 0.5f;

        /// <summary>
        /// 计算与另一地块的距离（几何中心间距离）
        /// </summary>
        public float DistanceTo(TileData other)
        {
            float dx = CenterX - other.CenterX;
            float dy = CenterY - other.CenterY;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>判断目标地块是否在辐射范围内</summary>
        public bool IsInRadiationRange(TileData other)
        {
            return RadiationRange > 0 && DistanceTo(other) <= RadiationRange;
        }
    }
}