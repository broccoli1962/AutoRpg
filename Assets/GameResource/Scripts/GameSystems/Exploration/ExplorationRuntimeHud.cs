using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
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
        [SerializeField] private bool _autoStartOnAwake = true;

        private Text _statusText;
        private Text _logText;
        private CompositeDisposable _disposables;
        private readonly System.Text.StringBuilder _logBuilder = new();

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

            ExplorationChannels.OnStateChanged
                .Subscribe(RefreshStatus)
                .AddTo(_disposables);

            if (_autoStartOnAwake)
            {
                ExplorationManager.ProcessOfflineElapsed();
                ExplorationManager.StartExploration();
            }

            RefreshStatus(ExplorationManager.GetCurrentState());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
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
            _logText = CreateText(canvasGo.transform, "LogText", new Vector2(20f, -120f), 16, TextAnchor.UpperLeft);
            _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _logText.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = _logText.rectTransform;
            rect.sizeDelta = new Vector2(Screen.width - 40f, Screen.height - 160f);
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
            if (_logBuilder.Length > 6000)
                _logBuilder.Clear();

            _logBuilder.AppendLine(entry.Text);
            _logText.text = _logBuilder.ToString();
        }

        private void RefreshStatus(ExplorationState state)
        {
            if (state == null)
            {
                _statusText.text = "탐험 상태 없음";
                return;
            }

            _statusText.text =
                $"{ZoneDefinitions.GetZoneDisplayName(state.ZoneId)} {state.CurrentFloor}층 · 진행 {state.FloorProgress:0.#}% · 골드 {state.Gold} · Tick {state.CurrentTick}";
        }
    }
}
