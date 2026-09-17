using UnityEngine;
using UnityEngine.UI;
using AmericanOfficialNotes.Controller;
using AmericanOfficialNotes.Model;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.View
{
    /// <summary>
    /// 地块信息面板 - 鼠标悬停显示运算过程
    /// 使用CanvasGroup控制显隐，避免SetActive导致自身被禁用后FindObjectOfType找不到
    /// </summary>
    public class TileInfoPanel : MonoBehaviour
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

        public void ShowTileInfo(int gridX, int gridY)
        {
            _tileX = gridX;
            _tileY = gridY;
            Show();
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
            if (tile == null) return;

            if (_titleText != null)
                _titleText.text = $"地块 ({_tileX}, {_tileY})";

            string info = $"分类: {tile.Category}\n等级: {tile.Level}\n辐射范围: {tile.RadiationRange:F1}";

            if (tile.HasBuilding)
            {
                info += $"\n\n建筑: {tile.Building.Type}";
                foreach (var paramType in tile.Building.GetParameterTypes())
                {
                    var detail = GameManager.Instance.BuildingController.GetParameterDetail(
                        _tileX, _tileY, paramType);
                    info += $"\n\n{paramType}: {detail.finalValue:F2}";
                    if (detail.modifiers.Count > 0)
                    {
                        info += $"\n  基础: {detail.baseValue:F2}";
                        foreach (var mod in detail.modifiers)
                            info += $"\n  {mod.SourceBuildingType}: {mod.ModifierValue:+0.00;-0.00}";
                    }
                }
            }
            else
            {
                info += "\n\n未建造";
            }

            _infoText.text = info;
        }
    }
}
