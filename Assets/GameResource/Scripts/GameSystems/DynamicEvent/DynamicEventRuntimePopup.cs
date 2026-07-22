using Backend.GameSystems.DynamicEvent.Data;
using Backend.Util;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.DynamicEvent
{
    /// <summary>
    /// Phase 4 동적 이벤트 Standard/Golden 강도 런타임 팝업. LLM 연출 장면과 선택지를 표시한다.
    /// </summary>
    public sealed class DynamicEventRuntimePopup : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _narrationText;
        [SerializeField] private Text _choicesText;
        [SerializeField] private RectTransform _choiceButtonRoot;
        [SerializeField] private Button _pauseForManualButton;
        private DynamicEventInstance _currentInstance;
        private CompositeDisposable _disposables;

        private void Awake()
        {
            if (_pauseForManualButton != null)
                _pauseForManualButton.onClick.AddListener(OnPauseForManualClicked);
        }

        private void Start()
        {
            Hide();

            _disposables = new CompositeDisposable();
            DynamicEventChannels.OnEventSceneReady
                .Subscribe(ShowScene)
                .AddTo(_disposables);

            DynamicEventChannels.OnEventResolved
                .Subscribe(_ => Hide())
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void ShowScene(DynamicEventInstance instance)
        {
            if (instance?.LlmNarration == null || _panelRoot == null || _titleText == null)
                return;

            _currentInstance = instance;
            ApplyPresentation(instance.Intensity == DynamicEventIntensity.Golden);

            _titleText.text = instance.Intensity == DynamicEventIntensity.Golden
                ? $"<color=#ffd966>[★ 황금 이벤트]</color> {instance.TemplateId} · {instance.Floor}층"
                : $"[이벤트] {instance.TemplateId} · {instance.Floor}층";
            _narrationText.text = instance.LlmNarration.Narration;

            _choicesText.text = instance.RequiresManualChoice
                ? "<b>선택지 (버튼 또는 1/2 키)</b>"
                : "<b>자동 진행 중 · 원하면 직접 선택</b>";

            RebuildChoiceButtons(instance);
            if (_pauseForManualButton != null)
                _pauseForManualButton.gameObject.SetActive(!instance.RequiresManualChoice);
            _panelRoot.SetActive(true);
        }

        private void RebuildChoiceButtons(DynamicEventInstance instance)
        {
            if (_choiceButtonRoot == null)
                return;

            for (var i = _choiceButtonRoot.childCount - 1; i >= 0; i--)
                Destroy(_choiceButtonRoot.GetChild(i).gameObject);

            var choices = instance.LlmNarration.Choices;
            if (choices == null)
                return;

            for (var i = 0; i < choices.Count; i++)
            {
                var choiceIndex = i;
                var choice = choices[i];
                var button = CreateChoiceButton(choiceIndex + 1, choice.Text);
                button.onClick.AddListener(() => DynamicEventManager.TrySubmitManualChoice(choiceIndex));
            }
        }

        private void OnPauseForManualClicked()
        {
            if (!DynamicEventManager.TryEnterManualChoiceMode())
                return;

            if (_currentInstance != null)
                ShowScene(_currentInstance);
        }

        private Button CreateChoiceButton(int number, string label)
        {
            var font = RuntimeUiFont.Get();
            var go = new GameObject($"Choice_{number}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_choiceButtonRoot, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180f, 72f);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.34f, 0.98f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 8f);
            labelRect.offsetMax = new Vector2(-8f, -8f);

            var text = labelGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = $"<b>{number}.</b> {label}";
            return button;
        }

        private void ApplyPresentation(bool isSpecial)
        {
            if (_panelRoot == null)
                return;

            var box = _panelRoot.transform.Find("Box");
            if (box == null)
                return;

            var boxRect = box.GetComponent<RectTransform>();
            var boxImage = box.GetComponent<Image>();
            if (boxRect == null || boxImage == null)
                return;

            if (isSpecial)
            {
                boxRect.anchorMin = Vector2.zero;
                boxRect.anchorMax = Vector2.one;
                boxRect.offsetMin = new Vector2(48f, 48f);
                boxRect.offsetMax = new Vector2(-48f, -48f);
                boxImage.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);
                _panelRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
                return;
            }

            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            boxRect.sizeDelta = new Vector2(720f, 420f);
            boxImage.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);
            _panelRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
        }

        private void Hide()
        {
            _currentInstance = null;
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }
    }
}
