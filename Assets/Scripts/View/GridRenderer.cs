using UnityEngine;
using AmericanOfficialNotes.Controller;

namespace AmericanOfficialNotes.View
{
    /// <summary>
    /// 网格渲染器 - 创建和管理地块GameObject
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        [SerializeField] private float _tileSize = 1f;
        [SerializeField] private GameObject _tilePrefab;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTileCreated += OnTileCreated;
                GameManager.Instance.OnTileRemoved += OnTileRemoved;
                GameManager.Instance.OnBuildingPlaced += OnBuildingChanged;
                GameManager.Instance.OnBuildingRemoved += OnBuildingChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnTileCreated -= OnTileCreated;
            GameManager.Instance.OnTileRemoved -= OnTileRemoved;
            GameManager.Instance.OnBuildingPlaced -= OnBuildingChanged;
            GameManager.Instance.OnBuildingRemoved -= OnBuildingChanged;
        }

        private static Sprite _whiteSprite;

        private static Sprite GetWhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;
            var tex = Texture2D.whiteTexture;
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);
            return _whiteSprite;
        }

        private void OnTileCreated(Model.TileData tile)
        {
            GameObject go;
            if (_tilePrefab != null)
            {
                go = Instantiate(_tilePrefab, transform);
            }
            else
            {
                go = new GameObject();
                go.transform.SetParent(transform);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = GetWhiteSprite();
                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(_tileSize, _tileSize);
            }
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(
                tile.GridX * _tileSize + _tileSize * 0.5f,
                tile.GridY * _tileSize + _tileSize * 0.5f, 0);
            go.transform.localScale = new Vector3(_tileSize * 0.9f, _tileSize * 0.9f, 1);
            go.name = $"Tile_{tile.GridX}_{tile.GridY}";
            var view = go.GetComponent<TileView>();
            if (view == null) view = go.AddComponent<TileView>();
            var bldView = go.GetComponent<BuildingView>();
            if (bldView == null) bldView = go.AddComponent<BuildingView>();
            bldView.Initialize(tile.GridX, tile.GridY);
            view.Initialize(tile.GridX, tile.GridY);
        }

        private void OnTileRemoved(Model.TileData tile)
        {
            var go = transform.Find($"Tile_{tile.GridX}_{tile.GridY}");
            if (go != null) Destroy(go.gameObject);
        }

        private void OnBuildingChanged(Model.TileData tile)
        {
            var child = transform.Find($"Tile_{tile.GridX}_{tile.GridY}");
            if (child != null)
            {
                var view = child.GetComponent<TileView>();
                if (view != null) view.UpdateAppearance();
                var bldView = child.GetComponent<BuildingView>();
                if (bldView != null) bldView.RefreshBuilding();
            }
        }

        /// <summary>生成测试网格（临时方法，后续由道路系统划分地块）</summary>
        public void GenerateTestGrid(int width, int height)
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    gm.TileController.CreateTile(x, y, Model.Enums.TileCategory.Residential);
        }
    }
}