using UnityEngine;
using UnityEngine.UI;
using AmericanOfficialNotes.Controller;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.View
{
    /// <summary>
    /// 输入处理 - 玩家输入转发给Controller
    /// 同时管理鼠标网格高亮指示器
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        private SpriteRenderer _cursorHighlight;

        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 15f;
        [SerializeField] private float _zoomSpeed = 1.5f;

        private void Start()
        {
            var tex = Texture2D.whiteTexture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);

            var go = new GameObject("CursorHighlight");
            _cursorHighlight = go.AddComponent<SpriteRenderer>();
            _cursorHighlight.sprite = sprite;
            _cursorHighlight.color = new Color(1f, 0.9f, 0.2f, 0.25f);
            _cursorHighlight.sortingOrder = 100;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1);
        }

        private void Update()
        {
            // 每帧更新鼠标高亮位置
            UpdateCursorHighlight();

            // 鼠标滚轮缩放
            HandleZoom();

            // 鼠标中键拖拽平移画布
            HandlePan();

            // 空格键推进一天
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.TimeController.AdvanceDay();
            }

            // T键创建测试地块
            if (Input.GetKeyDown(KeyCode.T))
            {
                var grid = FindObjectOfType<GridRenderer>();
                if (grid != null)
                    grid.GenerateTestGrid(5, 5);
            }

            // B键在选中地块放置建筑
            if (Input.GetKeyDown(KeyCode.B))
            {
                PlaceTestBuilding();
            }

            // R键拆除选中地块
            if (Input.GetKeyDown(KeyCode.R))
            {
                RemoveTile();
            }

            // 鼠标左键点击地块 - 显示信息面板
            if (Input.GetMouseButtonDown(0))
            {
                HandleTileClick();
            }

            // 1-5键切换建筑分类并高亮UI
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _selectedCategory = TileCategory.Residential;
                HighlightConstructionUI();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _selectedCategory = TileCategory.Commercial;
                HighlightConstructionUI();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _selectedCategory = TileCategory.Agricultural;
                HighlightConstructionUI();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _selectedCategory = TileCategory.Industrial;
                HighlightConstructionUI();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _selectedCategory = TileCategory.SocialService;
                HighlightConstructionUI();
            }
        }

        private TileCategory _selectedCategory = TileCategory.Residential;

        /// <summary>更新鼠标网格高亮指示器位置</summary>
        private void UpdateCursorHighlight()
        {
            if (_cursorHighlight == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            int gridX = Mathf.FloorToInt(pos.x);
            int gridY = Mathf.FloorToInt(pos.y);

            _cursorHighlight.transform.position = new Vector3(gridX + 0.5f, gridY + 0.5f, 0);
        }

        /// <summary>鼠标滚轮缩放相机</summary>
        private void HandleZoom()
        {
            var cam = Camera.main;
            if (cam == null || !cam.orthographic) return;

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Approximately(scroll, 0)) return;

            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - scroll * _zoomSpeed,
                _minZoom, _maxZoom);
        }

       private Vector3 _panOriginScreen;
        private Vector3 _panOriginCam;
        private bool _isPanning;
        private Vector3 _camSmoothVel;
        private Vector3 _targetCamPos = new Vector3(2.5f, 2.5f, -10f);

        public float panSmoothTime = 0.06f; // 越小越跟手

        /// <summary>鼠标中键拖拽平移画布</summary>
        private void HandlePan()
        {
            var cam = Camera.main;
            if (cam == null) return;

            if (Input.GetMouseButtonDown(2))
            {
                _isPanning = true;
                _panOriginScreen = Input.mousePosition;
                _panOriginCam = cam.transform.position;
                _targetCamPos = _panOriginCam;
            }
            else if (Input.GetMouseButtonUp(2))
            {
                _isPanning = false;
            }

            if (_isPanning)
            {
                Vector3 screenDelta = (Vector3)Input.mousePosition - _panOriginScreen;
                float worldUnitsPerPixel = cam.orthographicSize * 2f / Screen.height;
                Vector3 worldDelta = new Vector3(
                    -screenDelta.x * worldUnitsPerPixel,
                    -screenDelta.y * worldUnitsPerPixel, 0);
                _targetCamPos = _panOriginCam + worldDelta;
            }

            cam.transform.position = Vector3.SmoothDamp(
                cam.transform.position, _targetCamPos, ref _camSmoothVel, panSmoothTime);
        }

        /// <summary>获取鼠标点击的网格坐标</summary>
        private bool GetMouseGridPos(out int gridX, out int gridY)
        {
            gridX = 0;
            gridY = 0;

            var cam = Camera.main;
            if (cam == null) return false;

            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            gridX = Mathf.FloorToInt(pos.x);
            gridY = Mathf.FloorToInt(pos.y);
            return true;
        }

        private void PlaceTestBuilding()
        {
            if (GameManager.Instance == null) return;
            if (!GetMouseGridPos(out int x, out int y)) return;

            var tile = GameManager.Instance.TileController.GetTile(x, y);
            if (tile == null)
            {
                GameManager.Instance.TileController.CreateTile(x, y, _selectedCategory);
                tile = GameManager.Instance.TileController.GetTile(x, y);
            }
            if (tile == null || tile.HasBuilding) return;

            // 占位：根据分类放置第一个建筑类型
            BuildingType type = GetDefaultBuildingType(_selectedCategory);
            GameManager.Instance.BuildingController.PlaceBuilding(x, y, type);
        }

        private void RemoveTile()
        {
            if (GameManager.Instance == null) return;
            if (!GetMouseGridPos(out int x, out int y)) return;
            GameManager.Instance.TileController.RemoveTile(x, y);
        }

        /// <summary>鼠标左键点击 - 显示地块和建筑信息</summary>
        private void HandleTileClick()
        {
            if (GameManager.Instance == null) return;
            if (!GetMouseGridPos(out int x, out int y))return;
            // 判断鼠标点击区块是否存在地块
            var clickTile = GameManager.Instance.TileController.GetTile(x, y);
            var tilePanel = FindObjectOfType<TileInfoPanel>();
            var bldPanel = FindObjectOfType<BuildingInfoPanel>();


            if (clickTile == null)
            {
                tilePanel?.Hide();
                bldPanel?.Hide();
                return;
            }
            
            if (tilePanel != null)
                tilePanel.ShowTileInfo(x, y);

            if (bldPanel != null)
                bldPanel.ShowBuildingInfo(x, y);


        }

        /// <summary>高亮建筑分类UI中当前选中项</summary>
        private void HighlightConstructionUI()
        {
            var ui = FindObjectOfType<ConstructionInformationUI>();
            if (ui != null)
                ui.HighlightCategory(_selectedCategory);
        }

        private BuildingType GetDefaultBuildingType(TileCategory category)
        {
            switch (category)
            {
                case TileCategory.Residential:
                    return BuildingType.RuralResidential;
                case TileCategory.Commercial:
                    return BuildingType.SmallSupermarket;
                case TileCategory.Agricultural:
                    return BuildingType.NormalFarmland;
                case TileCategory.Industrial:
                    return BuildingType.SmallAgriculturalIndustry;
                case TileCategory.SocialService:
                    return BuildingType.Hospital;
                default:
                    return BuildingType.RuralResidential;
            }
        }
    }
}