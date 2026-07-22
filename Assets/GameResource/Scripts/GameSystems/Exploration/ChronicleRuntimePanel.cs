using Backend.GameSystems.Prestige;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 프로토타입 연대기(회차 회고록) 런타임 패널.
    /// </summary>
    public sealed class ChronicleRuntimePanel : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Text _contentText;
        private bool _isVisible;

        public bool IsVisible => _isVisible;

        private void Start()
        {
            BuildUi();
            Hide();
        }

        public void Toggle()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        private void BuildUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _panelRoot = new GameObject("ChroniclePanel");
            _panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 480f);

            var panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.14f, 0.96f);

            var title = CreateText(_panelRoot.transform, "Title", new Vector2(20f, -16f), 22, "[ 연대기 ]");
            title.rectTransform.sizeDelta = new Vector2(720f, 32f);

            _contentText = CreateText(_panelRoot.transform, "Content", new Vector2(20f, -56f), 16, string.Empty);
            _contentText.rectTransform.sizeDelta = new Vector2(720f, 400f);
            _contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _contentText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPos, int fontSize, string initial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(720f, 40f);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;
            text.text = initial;
            return text;
        }

        private void Show()
        {
            var meta = PrestigeManager.GetMeta();
            if (meta?.ChronicleEntries == null || meta.ChronicleEntries.Count == 0)
            {
                _contentText.text = "아직 기록된 회차가 없습니다.\n탐험을 마치면 연대기가 쌓입니다.";
            }
            else
            {
                var builder = new System.Text.StringBuilder();
                for (var i = meta.ChronicleEntries.Count - 1; i >= 0; i--)
                {
                    builder.Append("• ");
                    builder.AppendLine(meta.ChronicleEntries[i]);
                }

                _contentText.text = builder.ToString();
            }

            _panelRoot.SetActive(true);
            _isVisible = true;
        }

        private void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            _isVisible = false;
        }
    }
}
