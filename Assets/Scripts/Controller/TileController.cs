using System.Collections.Generic;
using System.Linq;
using AmericanOfficialNotes.Model;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 地块管理 - 创建/拆除/查询、距离计算
    /// 地块的本质是网格（平面直角坐标系）
    /// </summary>
    public class TileController
    {
        private readonly GameSaveData _gameData;
        private readonly GameManager _gameManager;

        public TileController(GameSaveData gameData, GameManager gameManager)
        {
            _gameData = gameData;
            _gameManager = gameManager;
        }

        /// <summary>创建地块（一经设置分类不可更改，除非手动拆除）</summary>
        public TileData CreateTile(int gridX, int gridY, TileCategory category, int level = 1)
        {
            if (GetTile(gridX, gridY) != null) return null;

            var tile = new TileData
            {
                GridX = gridX,
                GridY = gridY,
                Category = category,
                Level = level,
                BuildTimeDays = level,
                RadiationRange = 0f
            };
            _gameData.Tiles.Add(tile);
            _gameManager.InvokeTileCreated(tile);
            return tile;
        }

        /// <summary>拆除地块（手动拆除）</summary>
        public bool RemoveTile(int gridX, int gridY)
        {
            var tile = GetTile(gridX, gridY);
            if (tile == null) return false;

            if (tile.HasBuilding)
                _gameManager.BuildingController.RemoveBuilding(gridX, gridY);

            _gameData.Tiles.Remove(tile);
            _gameManager.InvokeTileRemoved(tile);
            return true;
        }

        /// <summary>获取地块</summary>
        public TileData GetTile(int gridX, int gridY)
        {
            return _gameData.Tiles.Find(t => t.GridX == gridX && t.GridY == gridY);
        }

        /// <summary>获取所有地块</summary>
        public List<TileData> GetAllTiles() => _gameData.Tiles;

        /// <summary>获取辐射范围内的所有地块</summary>
        public List<TileData> GetTilesInRadiationRange(int gridX, int gridY, float range)
        {
            var center = GetTile(gridX, gridY);
            if (center == null) return new List<TileData>();

            return _gameData.Tiles
                .Where(t => t != center && center.DistanceTo(t) <= range)
                .ToList();
        }

        /// <summary>计算两地块间距离（几何中心间距离）</summary>
        public float CalculateDistance(int x1, int y1, int x2, int y2)
        {
            var t1 = GetTile(x1, y1);
            var t2 = GetTile(x2, y2);
            if (t1 == null || t2 == null) return float.MaxValue;
            return t1.DistanceTo(t2);
        }
    }
}