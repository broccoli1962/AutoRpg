using Backend.GameSystems.Prestige;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 프로토타입 연대기(회차 회고록 + 즐겨찾기 순간) 런타임 패널.
    /// </summary>
    public sealed class ChronicleRuntimePanel : MonoBehaviour
    {
        private const int EntriesPerPage = 4;

        private GameObject _panelRoot;
        private Text _contentText;
        private Text _pageText;
        private bool _isVisible;
        private bool _showChronicle = true;
        private int _pageFromEnd;

        public bool IsVisible => _isVisible;

        private void Start()
        {
            BuildUi();
            Hide();
        }

        private void Update()
        {
            if (!_isVisible)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                _showChronicle = true;
                _pageFromEnd = 0;
                RefreshContent();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                _showChronicle = false;
                _pageFromEnd = 0;
                RefreshContent();
            }

            if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.LeftBracket))
                MovePage(older: true);

            if (Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.RightBracket))
                MovePage(older: false);
        }

        private void MovePage(bool older)
        {
            var entryCount = GetEntryCount();
            var totalPages = GetPageCount(entryCount);
            if (totalPages <= 1)
                return;

            if (older)
                _pageFromEnd = Mathf.Min(_pageFromEnd + 1, totalPages - 1);
            else
                _pageFromEnd = Mathf.Max(_pageFromEnd - 1, 0);

            RefreshContent();
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

            var hint = CreateText(_panelRoot.transform, "Hint", new Vector2(20f, -44f), 13,
                "1:회차 기록  2:즐겨찾기  PgUp/PgDn 또는 [/]:페이지");
            hint.rectTransform.sizeDelta = new Vector2(720f, 20f);
            hint.color = new Color(0.75f, 0.75f, 0.8f);

            _pageText = CreateText(_panelRoot.transform, "Page", new Vector2(20f, -58f), 12, string.Empty);
            _pageText.rectTransform.sizeDelta = new Vector2(720f, 18f);
            _pageText.color = new Color(0.65f, 0.65f, 0.72f);

            _contentText = CreateText(_panelRoot.transform, "Content", new Vector2(20f, -78f), 16, string.Empty);
            _contentText.rectTransform.sizeDelta = new Vector2(720f, 378f);
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
            _pageFromEnd = 0;
            RefreshContent();
            _panelRoot.SetActive(true);
            _isVisible = true;
        }

        private void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            _isVisible = false;
        }

        private void RefreshContent()
        {
            var meta = PrestigeManager.GetMeta();
            if (meta == null)
            {
                _contentText.text = "메타 진행 데이터가 없습니다.";
                _pageText.text = string.Empty;
                return;
            }

            if (_showChronicle)
            {
                if (meta.ChronicleEntries == null || meta.ChronicleEntries.Count == 0)
                {
                    _contentText.text = "아직 기록된 회차가 없습니다.\n탐험을 마치면 연대기가 쌓입니다.";
                    _pageText.text = string.Empty;
                    return;
                }

                RenderPagedEntries(
                    meta.ChronicleEntries,
                    "<b>[ 회차 연대기 ]</b>",
                    entry => entry,
                    bulletPrefix: "• ");
                return;
            }

            if (meta.FavoriteMoments == null || meta.FavoriteMoments.Count == 0)
            {
                _contentText.text = "즐겨찾기한 순간이 없습니다.\n로그에서 B키로 북마크할 수 있습니다.";
                _pageText.text = string.Empty;
                return;
            }

            RenderPagedEntries(
                meta.FavoriteMoments,
                "<b>[ 즐겨찾기 순간 ]</b>",
                entry => $"<color=#ffd966>★</color> {entry}",
                bulletPrefix: null);
        }

        private void RenderPagedEntries(
            System.Collections.Generic.List<string> entries,
            string header,
            System.Func<string, string> formatEntry,
            string bulletPrefix)
        {
            var totalPages = GetPageCount(entries.Count);
            _pageFromEnd = Mathf.Clamp(_pageFromEnd, 0, Mathf.Max(0, totalPages - 1));

            var endExclusive = entries.Count - _pageFromEnd * EntriesPerPage;
            var startInclusive = Mathf.Max(0, endExclusive - EntriesPerPage);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine(header);
            for (var i = endExclusive - 1; i >= startInclusive; i--)
            {
                if (!string.IsNullOrEmpty(bulletPrefix))
                    builder.Append(bulletPrefix);

                builder.AppendLine(formatEntry(entries[i]));
            }

            _contentText.text = builder.ToString();
            _pageText.text = totalPages > 1
                ? $"페이지 {totalPages - _pageFromEnd}/{totalPages}"
                : string.Empty;
        }

        private int GetEntryCount()
        {
            var meta = PrestigeManager.GetMeta();
            if (meta == null)
                return 0;

            if (_showChronicle)
                return meta.ChronicleEntries?.Count ?? 0;

            return meta.FavoriteMoments?.Count ?? 0;
        }

        private static int GetPageCount(int entryCount)
        {
            if (entryCount <= 0)
                return 1;

            return Mathf.CeilToInt(entryCount / (float)EntriesPerPage);
        }
    }
}
