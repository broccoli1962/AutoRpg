using System.Text;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX 중앙 패널 — 구역/층 진행·파티 요약·진행바.
    /// </summary>
    public sealed class ExplorationCenterRuntimePanel : MonoBehaviour
    {
        private Image _zoneBackdrop;
        private Text _zoneTitleText;
        private Text _floorText;
        private Slider _progressSlider;
        private Text _progressText;
        private Text _partyStripText;
        private Text _statusText;
        private CompositeDisposable _disposables;
        private readonly StringBuilder _builder = new();

        private void Start()
        {
            BuildUi();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            Refresh(ExplorationManager.GetCurrentState());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void BuildUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            var root = new GameObject("CenterPanel");
            root.transform.SetParent(canvas.transform, false);

            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(ExplorationHudLayoutMetrics.CenterPanelLeft, 0f);
            rect.sizeDelta = new Vector2(
                ExplorationHudLayoutMetrics.CenterPanelWidth,
                -(ExplorationHudLayoutMetrics.TopBarHeight + ExplorationHudLayoutMetrics.BottomInsetPx));

            var image = root.AddComponent<Image>();
            image.color = new Color(0.07f, 0.09f, 0.12f, 0.82f);

            _zoneBackdrop = CreateBackdrop(root.transform);
            _zoneTitleText = CreateText(root.transform, "ZoneTitle", new Vector2(16f, -16f), 24, "[ 탐험 진행 ]");
            _zoneTitleText.rectTransform.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.CenterPanelWidth - 32f, 32f);

            _floorText = CreateText(root.transform, "Floor", new Vector2(16f, -52f), 18, string.Empty);
            _floorText.rectTransform.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.CenterPanelWidth - 32f, 24f);

            _progressSlider = CreateProgressSlider(root.transform, new Vector2(16f, -84f));
            _progressText = CreateText(root.transform, "ProgressLabel", new Vector2(16f, -108f), 14, string.Empty);
            _progressText.color = new Color(0.75f, 0.82f, 0.95f);

            _partyStripText = CreateText(root.transform, "PartyStrip", new Vector2(16f, -136f), 15, string.Empty);
            _partyStripText.rectTransform.sizeDelta = new Vector2(
                ExplorationHudLayoutMetrics.CenterPanelWidth - 32f,
                120f);
            _partyStripText.lineSpacing = 1.15f;

            _statusText = CreateText(
                root.transform,
                "Status",
                new Vector2(16f, -(ExplorationHudLayoutMetrics.CenterPanelWidth > 320f ? 268f : 240f)),
                13,
                string.Empty);
            _statusText.rectTransform.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.CenterPanelWidth - 32f, 48f);
            _statusText.color = new Color(0.7f, 0.75f, 0.82f);
        }

        private void Refresh(ExplorationState state)
        {
            if (_zoneTitleText == null)
                return;

            if (state == null)
            {
                _zoneTitleText.text = "탐험 대기";
                _floorText.text = string.Empty;
                _partyStripText.text = string.Empty;
                _statusText.text = string.Empty;
                SetProgressVisible(false);
                return;
            }

            if (_zoneBackdrop != null)
                _zoneBackdrop.color = GetZoneTint(state.ZoneId);

            _zoneTitleText.text = ZoneDefinitions.GetZoneDisplayName(state.ZoneId);
            _floorText.text = $"현재 {state.CurrentFloor} / {state.MaxFloor} 층";

            if (_progressSlider != null)
            {
                _progressSlider.gameObject.SetActive(state.IsExploring);
                _progressSlider.value = Mathf.Clamp01(state.FloorProgress / 100f);
            }

            if (_progressText != null)
            {
                _progressText.gameObject.SetActive(state.IsExploring);
                _progressText.text = $"층 진행 {state.FloorProgress:0.#}% · Tick {state.CurrentTick}";
            }

            _partyStripText.text = BuildPartyStrip(state);
            _statusText.text = BuildStatusLine(state);
        }

        private void SetProgressVisible(bool visible)
        {
            if (_progressSlider != null)
                _progressSlider.gameObject.SetActive(visible);

            if (_progressText != null)
                _progressText.gameObject.SetActive(visible);
        }

        private static string BuildPartyStrip(ExplorationState state)
        {
            var members = state.Party?.Members;
            if (members == null || members.Count == 0)
                return "파티 없음";

            var builder = new StringBuilder();
            builder.AppendLine("<b>파티</b>");
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (i > 0)
                    builder.AppendLine();

                if (i == 0)
                    builder.Append("★ ");

                builder.Append(member.DisplayName);
                builder.Append("  HP ");
                builder.Append(member.CurrentHp);
                builder.Append('/');
                builder.Append(member.MaxHp);
            }

            return builder.ToString();
        }

        private static string BuildStatusLine(ExplorationState state)
        {
            if (!state.IsExploring)
                return "탐험이 종료되었습니다.";

            if (state.IsPaused)
                return "일시정지 중 · R 또는 귀환 버튼으로 재개/귀환";

            if (DynamicEventManager.IsAwaitingManualChoice)
                return "이벤트 선택 대기 · 1/2 키 또는 팝업에서 선택";

            return "자동 탐험 진행 중";
        }

        private static Color GetZoneTint(string zoneId)
        {
            return zoneId switch
            {
                ZoneDefinitions.FungalMazeId => new Color(0.16f, 0.28f, 0.18f, 0.55f),
                ZoneDefinitions.CrystalCavernId => new Color(0.15f, 0.22f, 0.38f, 0.55f),
                ZoneDefinitions.MoltenDepthsId => new Color(0.34f, 0.16f, 0.12f, 0.55f),
                ZoneDefinitions.SilentRuinsId => new Color(0.2f, 0.18f, 0.26f, 0.55f),
                ZoneDefinitions.AbyssalThresholdId => new Color(0.12f, 0.1f, 0.18f, 0.6f),
                _ => new Color(0.14f, 0.24f, 0.2f, 0.55f)
            };
        }

        private static Image CreateBackdrop(Transform parent)
        {
            var go = new GameObject("ZoneBackdrop");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 8f);
            rect.offsetMax = new Vector2(-8f, -8f);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.14f, 0.24f, 0.2f, 0.55f);
            return image;
        }

        private static Slider CreateProgressSlider(Transform parent, Vector2 anchoredPos)
        {
            var width = ExplorationHudLayoutMetrics.CenterPanelWidth - 32f;
            var go = new GameObject("CenterProgressSlider");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(width, 18f);

            var background = new GameObject("Background");
            background.transform.SetParent(go.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            StretchFull(bgRect);
            background.AddComponent<Image>().color = new Color(0.16f, 0.18f, 0.22f, 0.95f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            StretchFull(fillAreaRect);
            fillAreaRect.offsetMin = new Vector2(3f, 3f);
            fillAreaRect.offsetMax = new Vector2(-3f, -3f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            StretchFull(fillRect);
            fill.AddComponent<Image>().color = new Color(0.42f, 0.72f, 0.95f, 1f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fill.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPos, int fontSize, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;

            var label = go.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.UpperLeft;
            label.color = Color.white;
            label.supportRichText = true;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }
    }
}
