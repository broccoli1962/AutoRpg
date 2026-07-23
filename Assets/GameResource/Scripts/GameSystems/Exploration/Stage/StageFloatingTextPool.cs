using System.Collections.Generic;
using Backend.Util;
using TMPro;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// 스테이지 데미지/드롭 플로팅 텍스트 풀.
    /// </summary>
    public sealed class StageFloatingTextPool
    {
        private readonly RectTransform _root;
        private readonly Stack<TextMeshProUGUI> _pool = new();
        private readonly int _fontSize;

        public StageFloatingTextPool(RectTransform root, int fontSize = 22)
        {
            _root = root;
            _fontSize = fontSize;
        }

        public TextMeshProUGUI Rent()
        {
            if (_pool.Count > 0)
            {
                var pooled = _pool.Pop();
                pooled.gameObject.SetActive(true);
                return pooled;
            }

            var go = new GameObject("FloatText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 36f);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = _fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        public void Release(TextMeshProUGUI text)
        {
            if (text == null)
                return;

            text.gameObject.SetActive(false);
            _pool.Push(text);
        }
    }
}
