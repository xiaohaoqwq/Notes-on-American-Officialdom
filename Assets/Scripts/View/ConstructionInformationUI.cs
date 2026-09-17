using System;
using UnityEngine;
using UnityEngine.UI;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.View
{
    /// <summary>
    /// 建筑分类信息面板 - 轮询TileCategory枚举，显示分类列表
    /// 按下12345时高亮对应项
    /// </summary>
    public class ConstructionInformationUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _listContainer;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _highlightColor = new Color(1f, 0.9f, 0.2f);

        private Text[] _items;
        private int _selectedIndex = 0;
        private Font _font;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>初始化：轮询TileCategory枚举，创建文本项。由GameManager调用。</summary>
        public void Init()
        {
            if (_listContainer == null) CreateContainer();

            // 清除旧子项
            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);

            var categories = (TileCategory[])Enum.GetValues(typeof(TileCategory));
            _items = new Text[categories.Length];

            for (int i = 0; i < categories.Length; i++)
            {
                var cat = categories[i];
                int index = i + 1; // 显示从1开始
                var go = new GameObject($"Item_{index}_{cat}", typeof(RectTransform));
                go.transform.SetParent(_listContainer, false);

                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 1);
                rt.anchoredPosition = new Vector2(0, -i * 30);
                rt.sizeDelta = new Vector2(0, 28);

                var txt = go.AddComponent<Text>();
                txt.font = _font;
                txt.fontSize = 22;
                txt.color = _normalColor;
                txt.alignment = TextAnchor.MiddleLeft;
                txt.text = $"{index}. {GetCategoryDisplayName(cat)}";
                _items[i] = txt;
            }

            HighlightItem(0);
        }

        /// <summary>高亮指定索引项（0-based）</summary>
        public void HighlightItem(int index)
        {
            if (_items == null) return;
            for (int i = 0; i < _items.Length; i++)
                _items[i].color = (i == index) ? _highlightColor : _normalColor;
            _selectedIndex = index;
        }

        /// <summary>通过TileCategory值高亮</summary>
        public void HighlightCategory(TileCategory category)
        {
            var categories = (TileCategory[])Enum.GetValues(typeof(TileCategory));
            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i] == category)
                {
                    HighlightItem(i);
                    return;
                }
            }
        }

        private void CreateContainer()
        {
            var go = new GameObject("ListContainer", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);
            _listContainer = rt;
        }

        private static string GetCategoryDisplayName(TileCategory cat)
        {
            switch (cat)
            {
                case TileCategory.Residential: return "居民区";
                case TileCategory.Commercial: return "商业区";
                case TileCategory.Agricultural: return "农业畜牧";
                case TileCategory.Industrial: return "工业区";
                case TileCategory.SocialService: return "社会服务";
                default: return cat.ToString();
            }
        }
    }
}
