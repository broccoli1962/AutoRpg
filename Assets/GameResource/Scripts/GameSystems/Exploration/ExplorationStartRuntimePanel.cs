using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.Object.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 모바일 길드 대기 화면. 플레이어가 탭할 때까지 탐험을 시작하지 않는다.
    /// </summary>
    public sealed class ExplorationStartRuntimePanel : MonoBehaviour
    {
        private GameObject _overlayRoot;
        private Text _titleText;
        private Text _summaryText;
        private Button _startButton;
        private CompositeDisposable _disposables;

        private void Start()
        {
            BuildUi();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            ExplorationChannels.OnExplorationEnded
                .Subscribe(_ => Refresh(ExplorationManager.GetCurrentState()))
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

            _overlayRoot = new GameObject("ExplorationStartOverlay");
            _overlayRoot.transform.SetParent(canvas.transform, false);
            _overlayRoot.transform.SetAsLastSibling();

            var overlayRect = _overlayRoot.AddComponent<RectTransform>();
            StretchFull(overlayRect);

            var backdrop = _overlayRoot.AddComponent<Image>();
            backdrop.color = new Color(0.04f, 0.05f, 0.08f, 0.96f);
            backdrop.raycastTarget = true;

            var content = new GameObject("Content");
            content.transform.SetParent(_overlayRoot.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(560f, 420f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            _titleText = CreateText(content.transform, "Title", new Vector2(0f, -24f), 28, "자동 탐험 길드");
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.rectTransform.sizeDelta = new Vector2(contentRect.sizeDelta.x, 40f);

            _summaryText = CreateText(content.transform, "Summary", new Vector2(0f, -88f), 16, string.Empty);
            _summaryText.alignment = TextAnchor.UpperCenter;
            _summaryText.rectTransform.sizeDelta = new Vector2(contentRect.sizeDelta.x - 32f, 180f);
            _summaryText.color = new Color(0.82f, 0.86f, 0.92f);
            _summaryText.lineSpacing = 1.2f;

            _startButton = CreateStartButton(content.transform, font);
            _startButton.onClick.AddListener(OnStartClicked);
        }

        private void Refresh(ExplorationState state)
        {
            if (_overlayRoot == null)
                return;

            var show = state == null || !state.IsExploring;
            _overlayRoot.SetActive(show);

            if (!show)
                return;

            UpdateSummary(state);
        }

        private void UpdateSummary(ExplorationState state)
        {
            PrestigeManager.EnsureInitialized();
            var meta = PrestigeManager.GetMeta();
            var prestigeCount = Mathf.Max(1, (meta?.PrestigeCount ?? 0) + 1);
            var legacy = meta?.LegacyPoints ?? 0;
            var startingGold = PrestigeManager.GetStartingGoldBonus();
            var zoneName = ZoneDefinitions.GetZoneDisplayName(ZoneDefinitions.MossyHollowId);

            _summaryText.text =
                $"<b>제 {prestigeCount}회차 길드</b>\n" +
                $"누적 유산 {legacy}\n" +
                $"시작 골드 +{startingGold}\n\n" +
                $"다음 목적지: {zoneName}\n" +
                "준비가 끝나면 아래 버튼을 눌러 탐험을 시작하세요.";
        }

        private void OnStartClicked()
        {
            ExplorationManager.BeginExplorationFromPlayer();
        }

        private static Button CreateStartButton(Transform parent, Font font)
        {
            var go = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 32f);
            rect.sizeDelta = new Vector2(320f, 72f);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.24f, 0.52f, 0.82f, 1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            StretchFull(labelRect);

            var text = labelGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = "탐험 시작";
            return button;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            int fontSize,
            string initial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = initial;
            return text;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
