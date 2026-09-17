using UnityEngine;
using AmericanOfficialNotes.Controller;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.View
{
    /// <summary>
    /// 建筑视图 - 建筑prefab实例化、外观切换
    /// </summary>
    public class BuildingView : MonoBehaviour
    {
        [SerializeField] private Transform _buildingAnchor;

        private GameObject _currentBuilding;
        private int _tileX;
        private int _tileY;

        public void Initialize(int tileX, int tileY)
        {
            _tileX = tileX;
            _tileY = tileY;
        }

        public void RefreshBuilding()
        {
            if (GameManager.Instance == null) return;
            var tile = GameManager.Instance.TileController.GetTile(_tileX, _tileY);
            if (tile == null) return;

            if (_currentBuilding != null)
                Destroy(_currentBuilding);

            if (!tile.HasBuilding) return;

            // 占位：用Sprite表示不同建筑类型，后续替换为预制件引用
            var go = new GameObject(tile.Building.Type.ToString());
            go.transform.SetParent(_buildingAnchor != null ? _buildingAnchor : transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = GetBuildingColor(tile.Building.Type);
            sr.sortingOrder = 1;
            _currentBuilding = go;
        }

        private Color GetBuildingColor(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.RuralResidential:
                case BuildingType.TownshipResidential:
                case BuildingType.HighDensityResidential:
                    return new Color(0.4f, 0.3f, 0.2f);
                case BuildingType.SmallSupermarket:
                case BuildingType.LargeSupermarket:
                case BuildingType.IndividualDining:
                case BuildingType.ChainDining:
                    return new Color(0.2f, 0.5f, 0.8f);
                case BuildingType.NormalFarmland:
                case BuildingType.QualityFarmland:
                case BuildingType.ModernFarmland:
                    return new Color(0.3f, 0.7f, 0.2f);
                case BuildingType.Hospital:
                    return Color.red;
                case BuildingType.PoliceStation:
                    return Color.blue;
                case BuildingType.School:
                    return Color.yellow;
                default:
                    return Color.gray;
            }
        }
    }
}