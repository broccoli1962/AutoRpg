using System.Collections.Generic;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.LLM;
using Backend.Util;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Addressable HUD 없이 Phase 1 프로토타입을 검증하기 위한 런타임 디버그 HUD.
    /// </summary>
    public class ExplorationRuntimeHud : CachedMonobehaviour
    {
        private const int MaxLogLines = 500;
        private const int LogLinesPerPage = 36;

        [SerializeField] private bool _autoStartOnAwake = true;

        private Text _statusText;
        private Text _helpText;
        private Text _filterText;
        private Text _logText;
        private ChronicleRuntimePanel _chroniclePanel;
        private ExplorationSettingsRuntimePanel _settingsPanel;
        private CompositeDisposable _disposables;
        private readonly System.Text.StringBuilder _logBuilder = new();
        private readonly List<HudLogLine> _logLines = new();
        private readonly Dictionary<string, int> _indexByEventId = new();
        private LogFeedFilter _filter = LogFeedFilter.All;
        private int _logPageFromEnd;

        private void Awake()
        {
            BuildUi();
        }

        private void Start()
        {
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnLogAdded
                .Subscribe(AppendLog)
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

            PrestigeManager.EnsureInitialized();

            ExplorationChannels.OnStateChanged
                .Subscribe(RefreshStatus)
                .AddTo(_disposables);

            if (_autoStartOnAwake)
            {
                ExplorationManager.ProcessOfflineElapsed();
                ExplorationManager.StartExploration();
            }

            RefreshStatus(ExplorationManager.GetCurrentState());
            RefreshFilterLabel();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                LlmQualitySettings.CycleMode();
                RefreshStatus(ExplorationManager.GetCurrentState());
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                DynamicEventAutoPolicySettings.CyclePolicy();
                RefreshStatus(ExplorationManager.GetCurrentState());
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                GoldenEventSettings.ToggleAutoPause();
                RefreshStatus(ExplorationManager.GetCurrentState());
            }

            if (DynamicEventManager.IsAwaitingManualChoice)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    DynamicEventManager.TrySubmitManualChoice(0);

                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    DynamicEventManager.TrySubmitManualChoice(1);
            }

            if (Input.GetKeyDown(KeyCode.O))
                _settingsPanel?.Toggle();

            if (Input.GetKeyDown(KeyCode.C))
                _chroniclePanel?.Toggle();

            if (Input.GetKeyDown(KeyCode.R))
            {
                var state = ExplorationManager.GetCurrentState();
                if (state != null && state.IsExploring)
                    ExplorationManager.ReturnToGuild();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                _filter = LogFeedFilterUtil.Cycle(_filter);
                _logPageFromEnd = 0;
                RefreshFilterLabel();
                RebuildLogText();
            }

            if (Input.GetKeyDown(KeyCode.B))
                ToggleLastLogBookmark();

            if ((_chroniclePanel == null || !_chroniclePanel.IsVisible) &&
                (_settingsPanel == null || !_settingsPanel.IsVisible) &&
                Input.GetKeyDown(KeyCode.LeftBracket))
                MoveLogPage(older: true);

            if ((_chroniclePanel == null || !_chroniclePanel.IsVisible) &&
                (_settingsPanel == null || !_settingsPanel.IsVisible) &&
                Input.GetKeyDown(KeyCode.RightBracket))
                MoveLogPage(older: false);
        }

        private void MoveLogPage(bool older)
        {
            var filteredCount = CountFilteredLines();
            var totalPages = GetLogPageCount(filteredCount);
            if (totalPages <= 1)
                return;

            if (older)
                _logPageFromEnd = Mathf.Min(_logPageFromEnd + 1, totalPages - 1);
            else
                _logPageFromEnd = Mathf.Max(_logPageFromEnd - 1, 0);

            RebuildLogText();
        }

        private void ToggleLastLogBookmark()
        {
            if (_logLines.Count == 0)
                return;

            var index = _logLines.Count - 1;
            var line = _logLines[index];
            if (string.IsNullOrWhiteSpace(line.PlainText))
                return;

            line.IsBookmarked = LogBookmarkManager.Toggle(line.PlainText, line.Floor);
            line.RichText = line.BuildRichText();
            _logLines[index] = line;
            RebuildLogText();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("ExplorationRuntimeCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            _statusText = CreateText(canvasGo.transform, "StatusText", new Vector2(20f, -20f), 22, TextAnchor.UpperLeft);
            _helpText = CreateText(canvasGo.transform, "HelpText", new Vector2(20f, -88f), 14, TextAnchor.UpperLeft);
            _helpText.text = "L:LLM  A:이벤트  G:황금정지  O:설정  C:연대기  R:귀환  F:필터  B:북마크  [/]:로그페이지";
            _filterText = CreateText(canvasGo.transform, "FilterText", new Vector2(20f, -108f), 14, TextAnchor.UpperLeft);
            _logText = CreateText(canvasGo.transform, "LogText", new Vector2(20f, -132f), 16, TextAnchor.UpperLeft);
            _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _logText.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = _logText.rectTransform;
            rect.sizeDelta = new Vector2(Screen.width - 40f, Screen.height - 172f);

            canvasGo.AddComponent<DynamicEventRuntimePopup>();
            _chroniclePanel = canvasGo.AddComponent<ChronicleRuntimePanel>();
            _settingsPanel = canvasGo.AddComponent<ExplorationSettingsRuntimePanel>();
            _settingsPanel.Configure(() => RefreshStatus(ExplorationManager.GetCurrentState()));
            canvasGo.AddComponent<PartyRuntimePanel>();

            var logLeft = PartyRuntimePanel.PanelWidthPx + 24f;
            var logRect = _logText.rectTransform;
            logRect.anchoredPosition = new Vector2(logLeft, logRect.anchoredPosition.y);
            logRect.sizeDelta = new Vector2(Screen.width - logLeft - 20f, Screen.height - 172f);
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPos, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(Screen.width - 40f, 300f);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.supportRichText = true;
            return text;
        }

        private void AppendLog(LogEntry entry)
        {
            TrimIfNeeded();
            AddLine(HudLogLine.FromEntry(entry));
        }

        private void UpdateLog(LogEntry entry)
        {
            if (string.IsNullOrEmpty(entry.EventId) ||
                !_indexByEventId.TryGetValue(entry.EventId, out var index))
            {
                return;
            }

            _logLines[index] = HudLogLine.FromEntry(entry);
            RebuildLogText();
        }

        private void AppendDynamicEventLog(DynamicEvent.Data.DynamicEventInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.LlmResultNarration))
                return;

            TrimIfNeeded();
            AddLine(HudLogLine.FromDynamicEvent(instance.LlmResultNarration));
        }

        private void AppendPrestigeChronicle(Exploration.Simulation.ExplorationEndReason reason)
        {
            var meta = PrestigeManager.GetMeta();
            if (meta?.ChronicleEntries == null || meta.ChronicleEntries.Count == 0)
                return;

            var latest = meta.ChronicleEntries[meta.ChronicleEntries.Count - 1];
            TrimIfNeeded();
            AddLine(HudLogLine.FromChronicle(latest));
        }

        private void AddLine(HudLogLine line)
        {
            if (!string.IsNullOrEmpty(line.EventId))
                _indexByEventId[line.EventId] = _logLines.Count;

            _logLines.Add(line);
            if (_logPageFromEnd > 0)
            {
                var filteredCount = CountFilteredLines();
                _logPageFromEnd = Mathf.Clamp(_logPageFromEnd, 0, Mathf.Max(0, GetLogPageCount(filteredCount) - 1));
            }

            RebuildLogText();
        }

        private void TrimIfNeeded()
        {
            while (_logLines.Count >= MaxLogLines)
            {
                var removed = _logLines[0];
                _logLines.RemoveAt(0);

                if (!string.IsNullOrEmpty(removed.EventId))
                    _indexByEventId.Remove(removed.EventId);

                _indexByEventId.Clear();
                for (var i = 0; i < _logLines.Count; i++)
                {
                    var eventId = _logLines[i].EventId;
                    if (!string.IsNullOrEmpty(eventId))
                        _indexByEventId[eventId] = i;
                }
            }
        }

        private void RebuildLogText()
        {
            _logBuilder.Clear();
            var filteredCount = CountFilteredLines();
            var totalPages = GetLogPageCount(filteredCount);
            _logPageFromEnd = Mathf.Clamp(_logPageFromEnd, 0, Mathf.Max(0, totalPages - 1));

            var startIndex = 0;
            var endIndex = 0;
            GetVisibleRange(filteredCount, totalPages, out startIndex, out endIndex);

            var visibleIndex = 0;
            foreach (var line in _logLines)
            {
                if (!line.MatchesFilter(_filter))
                    continue;

                if (visibleIndex < startIndex)
                {
                    visibleIndex++;
                    continue;
                }

                if (visibleIndex >= endIndex)
                    break;

                _logBuilder.AppendLine(line.RichText);
                visibleIndex++;
            }

            if (totalPages > 1)
            {
                var currentPage = totalPages - _logPageFromEnd;
                _logBuilder.Insert(0, $"<color=#888888>[로그 {currentPage}/{totalPages}]</color>\n");
            }

            _logText.text = _logBuilder.ToString();
        }

        private int CountFilteredLines()
        {
            var count = 0;
            foreach (var line in _logLines)
            {
                if (line.MatchesFilter(_filter))
                    count++;
            }

            return count;
        }

        private static int GetLogPageCount(int filteredCount)
        {
            if (filteredCount <= 0)
                return 1;

            return Mathf.CeilToInt(filteredCount / (float)LogLinesPerPage);
        }

        private void GetVisibleRange(int filteredCount, int totalPages, out int startIndex, out int endIndex)
        {
            if (filteredCount <= 0)
            {
                startIndex = 0;
                endIndex = 0;
                return;
            }

            var pageFromEnd = Mathf.Clamp(_logPageFromEnd, 0, totalPages - 1);
            endIndex = filteredCount - pageFromEnd * LogLinesPerPage;
            startIndex = Mathf.Max(0, endIndex - LogLinesPerPage);
        }

        private void RefreshFilterLabel()
        {
            if (_filterText != null)
                _filterText.text = $"로그 필터: {LogFeedFilterUtil.GetDisplayLabel(_filter)} (F)";
        }

        private void RefreshStatus(ExplorationState state)
        {
            if (_statusText == null)
                return;

            _statusText.text = ExplorationHudStatusFormatter.Build(state);
        }

        private sealed class HudLogLine
        {
            public string EventId;
            public string PlainText;
            public int Floor;
            public bool IsBookmarked;
            public bool IsCombat;
            public bool IsDiscovery;
            public bool IsDynamicEvent;
            public bool IsNarrative;
            public string RichText;
            private string _baseRichText;

            public bool MatchesFilter(LogFeedFilter filter)
            {
                return filter switch
                {
                    LogFeedFilter.Combat => IsCombat,
                    LogFeedFilter.Discovery => IsDiscovery,
                    LogFeedFilter.Event => IsDynamicEvent,
                    LogFeedFilter.Narrative => IsNarrative,
                    _ => true
                };
            }

            public string BuildRichText() =>
                LogBookmarkManager.ApplyBookmarkPrefix(_baseRichText, IsBookmarked);

            public static HudLogLine FromEntry(LogEntry entry)
            {
                var line = new HudLogLine
                {
                    EventId = entry.EventId,
                    PlainText = entry.Text,
                    Floor = entry.Floor,
                    IsCombat = entry.Category == LogCategory.Combat,
                    IsDiscovery = entry.Category == LogCategory.Discovery,
                    IsDynamicEvent = false,
                    IsNarrative = entry.UsedLlm
                        || entry.Salience >= SalienceGrade.Significant
                        || entry.Category == LogCategory.Milestone,
                    _baseRichText = LogDisplayUtil.FormatRichText(entry)
                };
                line.IsBookmarked = LogBookmarkManager.IsBookmarked(line.PlainText, line.Floor);
                line.RichText = line.BuildRichText();
                return line;
            }

            public static HudLogLine FromDynamicEvent(string narration)
            {
                var state = ExplorationManager.GetCurrentState();
                var floor = state?.CurrentFloor ?? 0;
                var line = new HudLogLine
                {
                    PlainText = narration,
                    Floor = floor,
                    IsDynamicEvent = true,
                    IsNarrative = true,
                    _baseRichText = LogDisplayUtil.FormatTaggedLine(
                        "이벤트",
                        narration,
                        LogDisplayUtil.GetCategoryColor(LogCategory.Milestone))
                };
                line.IsBookmarked = LogBookmarkManager.IsBookmarked(line.PlainText, line.Floor);
                line.RichText = line.BuildRichText();
                return line;
            }

            public static HudLogLine FromChronicle(string text)
            {
                var state = ExplorationManager.GetCurrentState();
                var floor = state?.CurrentFloor ?? 0;
                var line = new HudLogLine
                {
                    PlainText = text,
                    Floor = floor,
                    IsNarrative = true,
                    _baseRichText = LogDisplayUtil.FormatTaggedLine(
                        "연대기",
                        text,
                        LogDisplayUtil.GetCategoryColor(LogCategory.Milestone))
                };
                line.IsBookmarked = LogBookmarkManager.IsBookmarked(line.PlainText, line.Floor);
                line.RichText = line.BuildRichText();
                return line;
            }
        }
    }
}
