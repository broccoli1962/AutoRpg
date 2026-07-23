using System.Collections.Generic;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Simulation;
using Backend.GameSystems.Prestige;
using Backend.Object.Management;
using Backend.Object.Management.Pool;
using Backend.Util;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Exploration
{
    public class ExplorationLogFeedView : UIView
    {
        private const int MaxLogLines = 500;
        private const int LogLinesPerPage = 36;
        private bool _stripMode;

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private ExplorationLogItemView _itemPrefab;
        [SerializeField] private GameObject _idlePlaceholderRoot;
        [SerializeField] private TextMeshProUGUI _idlePlaceholderText;

        private readonly List<ExplorationLogItemView> _items = new();
        private readonly Dictionary<string, ExplorationLogItemView> _itemsByEventId = new();
        private CompositeDisposable _disposables;
        private GameObject _idlePlaceholder;
        private LogFeedFilter _filter = LogFeedFilter.All;
        private int _pageFromEnd;
        private Pooling<ExplorationLogItemView> _itemPool;

        public LogFeedFilter CurrentFilter => _filter;

        /// <summary>스테이지 우선 HUD — 로그를 하단 스트립으로 축소.</summary>
        public void ApplyStripMode()
        {
            _stripMode = true;

            if (_scrollRect != null)
            {
                _scrollRect.movementType = ScrollRect.MovementType.Clamped;
                _scrollRect.scrollSensitivity = 18f;
            }

            RefreshStripItemTypography();
        }

        private void RefreshStripItemTypography()
        {
            foreach (var item in _items)
            {
                if (item == null)
                    continue;

                item.ApplyStripTypography(ExplorationHudLayoutMetrics.LogStripBodyFontSize);
            }
        }

        internal void ConfigureRuntime(
            ScrollRect scrollRect,
            RectTransform contentRoot,
            ExplorationLogItemView itemPrefab)
        {
            _scrollRect = scrollRect;
            _contentRoot = contentRoot;
            _itemPrefab = itemPrefab;
        }

        protected override void OnShow()
        {
            if (_idlePlaceholder != null)
                _idlePlaceholder.SetActive(false);

            if (_idlePlaceholderRoot != null)
                _idlePlaceholderRoot.SetActive(false);

            if (_scrollRect != null)
                _scrollRect.gameObject.SetActive(true);

            base.OnShow();
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            PrestigeManager.EnsureInitialized();

            ExplorationChannels.OnLogAdded
                .Subscribe(AddLog)
                .AddTo(_disposables);

            ExplorationChannels.OnLogUpdated
                .Subscribe(UpdateLog)
                .AddTo(_disposables);

            ExplorationChannels.OnLogStreaming
                .Subscribe(UpdateLog)
                .AddTo(_disposables);

            DynamicEventChannels.OnEventResolved
                .Subscribe(AppendDynamicEventLog)
                .AddTo(_disposables);

            ExplorationChannels.OnExplorationEnded
                .Subscribe(AppendPrestigeChronicle)
                .AddTo(_disposables);
        }

        protected override void OnHide()
        {
            base.OnHide();
            _disposables?.Dispose();
            _disposables = null;
        }

        public void ShowIdlePlaceholder()
        {
            _disposables?.Dispose();
            _disposables = null;

            EnsureIdlePlaceholder();
            if (_idlePlaceholderRoot != null)
                _idlePlaceholderRoot.SetActive(true);

            if (_scrollRect != null)
                _scrollRect.gameObject.SetActive(false);

            CachedGameObject.SetActive(true);
        }

        private void EnsureIdlePlaceholder()
        {
            if (_idlePlaceholderRoot != null || _contentRoot == null)
                return;

            _idlePlaceholder = new GameObject("IdlePlaceholder", typeof(RectTransform));
            _idlePlaceholder.transform.SetParent(_contentRoot.parent, false);
            var rect = _idlePlaceholder.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 12f);
            rect.offsetMax = new Vector2(-12f, -12f);

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(_idlePlaceholder.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _idlePlaceholderText = textGo.AddComponent<TextMeshProUGUI>();
            UiTmpUtil.ApplyLayoutCell(_idlePlaceholderText, RuntimeUiTmpFont.Get(), ExplorationHudLayoutMetrics.LogBodyFontSize, TextAnchor.UpperLeft, lineCount: 4, color: ModernUiStyle.MutedText);
            _idlePlaceholderText.text = "탐험 시작 전입니다.\n\n중앙 「탐험 시작」 버튼을 누르면\n실시간 탐험 로그가 표시됩니다.";
            _idlePlaceholderRoot = _idlePlaceholder;
        }

        public void CycleFilter()
        {
            _filter = LogFeedFilterUtil.Cycle(_filter);
            _pageFromEnd = 0;
            RefreshVisibility();
        }

        public void MovePage(bool older)
        {
            var filteredCount = CountFilteredItems();
            var totalPages = GetPageCount(filteredCount);
            if (totalPages <= 1)
                return;

            if (older)
                _pageFromEnd = Mathf.Min(_pageFromEnd + 1, totalPages - 1);
            else
                _pageFromEnd = Mathf.Max(_pageFromEnd - 1, 0);

            RefreshVisibility();
        }

        public void ToggleLastBookmark()
        {
            if (_items.Count == 0)
                return;

            var item = _items[_items.Count - 1];
            if (string.IsNullOrWhiteSpace(item.PlainText))
                return;

            var bookmarked = LogBookmarkSystem.Toggle(item.PlainText, item.Floor);
            item.SetBookmarked(bookmarked);
        }

        public void ClearLogs()
        {
            foreach (var item in _items)
                ReleaseLogItem(item);

            _items.Clear();
            _itemsByEventId.Clear();
            _pageFromEnd = 0;
        }

        private void EnsureItemPool()
        {
            if (_itemPool != null || _itemPrefab == null)
                return;

            _itemPool = ObjectPoolManager.GetOrCreatePool(
                _itemPrefab,
                _contentRoot,
                defaultCapacity: 32,
                maxSize: MaxLogLines + 8,
                onRelease: item =>
                {
                    if (item != null)
                        item.CachedGameObject.SetActive(false);
                });
        }

        private ExplorationLogItemView RentLogItem()
        {
            if (_itemPrefab == null || _contentRoot == null)
                return null;

            EnsureItemPool();
            var item = _itemPool?.Get();
            if (item == null)
                return null;

            item.transform.SetParent(_contentRoot, false);
            UiTmpUtil.EnsureLogItemStretch(item.transform as RectTransform);
            item.CachedGameObject.SetActive(true);
            return item;
        }

        private void ReleaseLogItem(ExplorationLogItemView item)
        {
            if (item == null)
                return;

            if (_itemPool != null)
                ObjectPoolManager.Release(item);
            else
                Destroy(item.CachedGameObject);
        }

        private void AddLog(LogEntry entry)
        {
            var item = RentLogItem();
            if (item == null)
                return;

            item.Bind(entry);
            if (_stripMode)
                item.ApplyStripTypography(ExplorationHudLayoutMetrics.LogStripBodyFontSize);
            _items.Add(item);

            if (!string.IsNullOrEmpty(entry.EventId))
                _itemsByEventId[entry.EventId] = item;

            TrimIfNeeded();
            if (_pageFromEnd == 0)
                ScrollToLatest();
            RefreshVisibility();
        }

        private void UpdateLog(LogEntry entry)
        {
            if (string.IsNullOrEmpty(entry.EventId))
                return;

            if (_itemsByEventId.TryGetValue(entry.EventId, out var item) && item != null)
            {
                item.Bind(entry);
                if (_pageFromEnd == 0)
                    ScrollToLatest();
                RefreshVisibility();
            }
        }

        private void AppendDynamicEventLog(DynamicEventInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.LlmResultNarration))
                return;

            var state = ExplorationSystem.GetCurrentState();
            var floor = state?.CurrentFloor ?? 0;
            AddTaggedLog(instance.LlmResultNarration, floor, "이벤트", LogCategory.Milestone, isDynamicEvent: true, isNarrative: true);
        }

        private void AppendPrestigeChronicle(ExplorationEndReason reason)
        {
            var meta = PrestigeManager.GetMeta();
            if (meta?.ChronicleEntries == null || meta.ChronicleEntries.Count == 0)
                return;

            var latest = meta.ChronicleEntries[meta.ChronicleEntries.Count - 1];
            var state = ExplorationSystem.GetCurrentState();
            var floor = state?.CurrentFloor ?? 0;
            AddTaggedLog(latest, floor, "연대기", LogCategory.Milestone, isDynamicEvent: false, isNarrative: true);
        }

        private void AddTaggedLog(
            string text,
            int floor,
            string tag,
            LogCategory category,
            bool isDynamicEvent,
            bool isNarrative)
        {
            var item = RentLogItem();
            if (item == null)
                return;

            item.BindTagged(text, floor, tag, category, isDynamicEvent, isNarrative);
            _items.Add(item);
            TrimIfNeeded();
            if (_pageFromEnd == 0)
                ScrollToLatest();
            RefreshVisibility();
        }

        private void TrimIfNeeded()
        {
            var maxLines = _stripMode
                ? Mathf.Min(MaxLogLines, ExplorationHudLayoutMetrics.LogStripMaxVisibleLines * 8)
                : MaxLogLines;

            while (_items.Count > maxLines)
            {
                var oldest = _items[0];
                _items.RemoveAt(0);
                if (oldest == null)
                    continue;

                if (!string.IsNullOrEmpty(oldest.EventId) &&
                    _itemsByEventId.TryGetValue(oldest.EventId, out var mapped) &&
                    mapped == oldest)
                {
                    _itemsByEventId.Remove(oldest.EventId);
                }

                ReleaseLogItem(oldest);
            }

            _itemsByEventId.Clear();
            for (var i = 0; i < _items.Count; i++)
            {
                var eventId = _items[i].EventId;
                if (!string.IsNullOrEmpty(eventId))
                    _itemsByEventId[eventId] = _items[i];
            }
        }

        private void RefreshVisibility()
        {
            var filteredCount = CountFilteredItems();
            var totalPages = GetPageCount(filteredCount);
            _pageFromEnd = Mathf.Clamp(_pageFromEnd, 0, Mathf.Max(0, totalPages - 1));

            var endExclusive = filteredCount - _pageFromEnd * LogLinesPerPage;
            var startInclusive = Mathf.Max(0, endExclusive - LogLinesPerPage);
            var visibleIndices = new HashSet<int>();
            var visibleRank = 0;

            for (var i = 0; i < _items.Count; i++)
            {
                if (!_items[i].MatchesFilter(_filter))
                {
                    _items[i].CachedGameObject.SetActive(false);
                    continue;
                }

                if (visibleRank >= startInclusive && visibleRank < endExclusive)
                    visibleIndices.Add(i);

                visibleRank++;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].MatchesFilter(_filter))
                    _items[i].CachedGameObject.SetActive(visibleIndices.Contains(i));
            }
        }

        private int CountFilteredItems()
        {
            var count = 0;
            foreach (var item in _items)
            {
                if (item != null && item.MatchesFilter(_filter))
                    count++;
            }

            return count;
        }

        private static int GetPageCount(int filteredCount)
        {
            if (filteredCount <= 0)
                return 1;

            return Mathf.CeilToInt(filteredCount / (float)LogLinesPerPage);
        }

        private void ScrollToLatest()
        {
            if (_scrollRect == null)
                return;

            if (_contentRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRoot);

            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
