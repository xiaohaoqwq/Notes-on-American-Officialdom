using UnityEngine;
using AmericanOfficialNotes.Controller;
using AmericanOfficialNotes.Model;

namespace AmericanOfficialNotes.View
{
    /// <summary>
    /// 地块视图 - 渲染地块、处理鼠标悬停/点击
    /// </summary>
    public class TileView : MonoBehaviour
    {
        public int GridX { get; private set; }
        public int GridY { get; private set; }

        private SpriteRenderer _renderer;
        private SpriteRenderer _borderRenderer;
        private Color _baseColor;
        private static readonly Color[] CategoryColors =
        {
            new Color(0.3f, 0.6f, 0.3f), // Residential - green
            new Color(0.3f, 0.5f, 0.8f), // Commercial - blue
            new Color(0.8f, 0.7f, 0.3f), // Agricultural - yellow
            new Color(0.5f, 0.5f, 0.5f), // Industrial - gray
            new Color(0.8f, 0.3f, 0.3f)  // SocialService - red
        };

        public void Initialize(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;
            _renderer = GetComponent<SpriteRenderer>();
            CreateBorder();
            UpdateAppearance();
        }

        private void CreateBorder()
        {
            if (transform.Find("Border") != null) return;

            var borderGo = new GameObject("Border");
            borderGo.transform.SetParent(transform, false);
            borderGo.transform.localPosition = Vector3.zero;
            borderGo.transform.localScale = new Vector3(
                1f / (transform.localScale.x == 0 ? 1 : transform.localScale.x),
                1f / (transform.localScale.y == 0 ? 1 : transform.localScale.y),
                1);

            var sr = borderGo.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(Texture2D.whiteTexture,
                new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            sr.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            sr.sortingOrder = -1;

            _borderRenderer = sr;
        }

        public void UpdateAppearance()
        {
            if (GameManager.Instance == null) return;
            var tile = GameManager.Instance.TileController.GetTile(GridX, GridY);
            if (tile == null || _renderer == null) return;

            _baseColor = CategoryColors[(int)tile.Category];
            if (tile.HasBuilding)
                _baseColor *= 1.5f;
            _renderer.color = _baseColor;
        }

        private static readonly Color BorderNormal = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        private static readonly Color BorderHighlight = new Color(1f, 0.9f, 0.2f, 1f);

        void OnMouseEnter()
        {
            if (_renderer != null)
                _renderer.color = _baseColor * 1.3f;
            if (_borderRenderer != null)
                _borderRenderer.color = BorderHighlight;
        }

        void OnMouseExit()
        {
            UpdateAppearance();
            if (_borderRenderer != null)
                _borderRenderer.color = BorderNormal;
        }

        void OnMouseDown()
        {
            if (GameManager.Instance == null) return;
            var tilePanel = FindObjectOfType<TileInfoPanel>();
            if (tilePanel != null)
                tilePanel.ShowTileInfo(GridX, GridY);

            var bldPanel = FindObjectOfType<BuildingInfoPanel>();
            if (bldPanel != null)
                bldPanel.ShowBuildingInfo(GridX, GridY);
        }
    }
}