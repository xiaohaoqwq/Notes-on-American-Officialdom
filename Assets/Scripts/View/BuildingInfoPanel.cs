using UnityEngine;
using UnityEngine.UI;
using AmericanOfficialNotes.Controller;

namespace AmericanOfficialNotes.View
{
    /// <summary>
    /// 建筑参数面板 - 显示修正后参数
    /// 使用CanvasGroup控制显隐
    /// </summary>
    public class BuildingInfoPanel : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _infoText;

        private CanvasGroup _canvasGroup;
        private int _tileX = -1;
        private int _tileY = -1;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Hide();
        }

        public void ShowBuildingInfo(int gridX, int gridY)
        {
            _tileX = gridX;
            _tileY = gridY;
            Show();
            this.transform.position = Input.mousePosition + new Vector3(10f, -10f, 0f); // 偏移鼠标位置

            RefreshDisplay();
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void Show()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        private void RefreshDisplay()
        {
            if (GameManager.Instance == null || _infoText == null) return;
            var tile = GameManager.Instance.TileController.GetTile(_tileX, _tileY);
            if (tile == null || !tile.HasBuilding) return;

            if (_titleText != null)
                _titleText.text = $"建筑参数 - {tile.Building.Type}";

            string info = "";
            foreach (var paramType in tile.Building.GetParameterTypes())
            {
                float value = GameManager.Instance.BuildingController.GetParameter(
                    _tileX, _tileY, paramType);
                info += $"{paramType}: {value:F2}\n";
            }
            _infoText.text = info;
        }
    }
}
