using System.Collections.Generic;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.DynamicEvent;
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
        private const int MaxLogLines = 120;

        [SerializeField] private bool _autoStartOnAwake = true;

        private Text _statusText;
        private Text _helpText;
        private Text _filterText;
        private Text _logText;
        private ChronicleRuntimePanel _chroniclePanel;
        private CompositeDisposable _disposables;
        private readonly System.Text.StringBuilder _logBuilder = new();
        private readonly List<HudLogLine> _logLines = new();
        private readonly Dictionary<string, int> _indexByEventId = new();
        private LogFeedFilter _filter = LogFeedFilter.All;

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
                RefreshFilterLabel();
                RebuildLogText();
            }
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
            _helpText.text = "L:LLM품질  C:연대기  R:귀환  F:로그필터";
            _filterText = CreateText(canvasGo.transform, "FilterText", new Vector2(20f, -108f), 14, TextAnchor.UpperLeft);
            _logText = CreateText(canvasGo.transform, "LogText", new Vector2(20f, -132f), 16, TextAnchor.UpperLeft);
            _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _logText.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = _logText.rectTransform;
            rect.sizeDelta = new Vector2(Screen.width - 40f, Screen.height - 172f);

            canvasGo.AddComponent<DynamicEventRuntimePopup>();
            _chroniclePanel = canvasGo.AddComponent<ChronicleRuntimePanel>();
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

            foreach (var line in _logLines)
            {
                if (!line.MatchesFilter(_filter))
                    continue;

                _logBuilder.AppendLine(line.RichText);
            }

            _logText.text = _logBuilder.ToString();
        }

        private void RefreshFilterLabel()
        {
            if (_filterText != null)
                _filterText.text = $"로그 필터: {LogFeedFilterUtil.GetDisplayLabel(_filter)} (F)";
        }

        private void RefreshStatus(ExplorationState state)
        {
            if (state == null)
            {
                _statusText.text = "탐험 상태 없음";
                return;
            }

            var meta = PrestigeManager.GetMeta();
            var equipment = EquipmentService.GetLeaderEquipmentSummary(state.Party);
            _statusText.text =
                $"{ZoneDefinitions.GetZoneDisplayName(state.ZoneId)} {state.CurrentFloor}층 · 진행 {state.FloorProgress:0.#}% · " +
                $"골드 {state.Gold} · 유산 {meta?.LegacyPoints ?? 0} · {LlmQualitySettings.GetDisplayLabel()}\n" +
                $"장비 {equipment} · Tick {state.CurrentTick}";
        }

        private sealed class HudLogLine
        {
            public string EventId;
            public bool IsCombat;
            public bool IsDiscovery;
            public bool IsDynamicEvent;
            public bool IsNarrative;
            public string RichText;

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

            public static HudLogLine FromEntry(LogEntry entry)
            {
                return new HudLogLine
                {
                    EventId = entry.EventId,
                    IsCombat = entry.Category == LogCategory.Combat,
                    IsDiscovery = entry.Category == LogCategory.Discovery,
                    IsDynamicEvent = false,
                    IsNarrative = entry.UsedLlm
                        || entry.Salience >= SalienceGrade.Significant
                        || entry.Category == LogCategory.Milestone,
                    RichText = LogDisplayUtil.FormatRichText(entry)
                };
            }

            public static HudLogLine FromDynamicEvent(string narration)
            {
                return new HudLogLine
                {
                    IsDynamicEvent = true,
                    IsNarrative = true,
                    RichText = LogDisplayUtil.FormatTaggedLine(
                        "이벤트",
                        narration,
                        LogDisplayUtil.GetCategoryColor(LogCategory.Milestone))
                };
            }

            public static HudLogLine FromChronicle(string text)
            {
                return new HudLogLine
                {
                    IsNarrative = true,
                    RichText = LogDisplayUtil.FormatTaggedLine(
                        "연대기",
                        text,
                        LogDisplayUtil.GetCategoryColor(LogCategory.Milestone))
                };
            }
        }
    }
}
