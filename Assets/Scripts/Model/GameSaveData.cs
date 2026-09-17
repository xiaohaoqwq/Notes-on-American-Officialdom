using System;
using System.Collections.Generic;

namespace AmericanOfficialNotes.Model
{
    /// <summary>
    /// 全局游戏存档数据
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>游戏当前天数</summary>
        public int CurrentDay { get; set; }

        /// <summary>资金（百万现金起手）</summary>
        public decimal Funds { get; set; }

        /// <summary>路线选择：人民至上、官商勾结、土地财政</summary>
        public string RouteChoice { get; set; }

        /// <summary>所有地块数据</summary>
        public List<TileData> Tiles { get; set; } = new List<TileData>();

        /// <summary>所有人口数据</summary>
        public List<PopulationData> Population { get; set; } = new List<PopulationData>();

        /// <summary>姓名库</summary>
        public NameLibrary NameLibrary { get; set; } = new NameLibrary();
    }
}