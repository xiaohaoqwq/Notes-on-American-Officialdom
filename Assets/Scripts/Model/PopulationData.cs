using System;

namespace AmericanOfficialNotes.Model
{
    /// <summary>
    /// 人口数据 - 纯C#数据结构，不依赖Unity
    /// </summary>
    [Serializable]
    public class PopulationData
    {
        /// <summary>唯一ID（UUID）</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>姓名</summary>
        public string Name { get; set; }

        /// <summary>性别（true=男，false=女）</summary>
        public bool IsMale { get; set; }

        /// <summary>居住地地块X坐标</summary>
        public int HomeTileX { get; set; }

        /// <summary>居住地地块Y坐标</summary>
        public int HomeTileY { get; set; }

        /// <summary>工作地块X坐标（-1表示未就业）</summary>
        public int WorkTileX { get; set; } = -1;

        /// <summary>工作地块Y坐标（-1表示未就业）</summary>
        public int WorkTileY { get; set; } = -1;

        /// <summary>健康（影响居民区参数）</summary>
        public float Health { get; set; }

        /// <summary>心情（影响居民区参数）</summary>
        public float Mood { get; set; }

        /// <summary>消费实力（影响居民区及商业区参数）</summary>
        public float ConsumptionPower { get; set; }

        /// <summary>劳动力值（下限0，上限由建筑和修正影响，起始0-100）</summary>
        public float Labor { get; set; }

        /// <summary>劳动力上限</summary>
        public float LaborMax { get; set; } = 100f;

        /// <summary>金钱（可供商业区消费）</summary>
        public float Money { get; set; }

        /// <summary>存款</summary>
        public float Savings { get; set; }

        /// <summary>上一帧劳动力值（用于UI显示变化趋势）</summary>
        public float PreviousLabor { get; set; }

        /// <summary>是否已就业</summary>
        public bool IsEmployed => WorkTileX >= 0 && WorkTileY >= 0;

        /// <summary>劳动力变化量（正=增长，负=减少）</summary>
        public float LaborDelta => Labor - PreviousLabor;
    }
}