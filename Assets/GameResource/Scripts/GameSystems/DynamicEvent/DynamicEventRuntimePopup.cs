using Backend.GameSystems.DynamicEvent.Data;
using Backend.Object.UI;
using Backend.Util;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.DynamicEvent
{
    /// <summary>
    /// Phase 4 동적 이벤트 Standard/Golden 강도 런타임 팝업. LLM 연출 장면과 선택지를 표시한다.
    /// </summary>
    public sealed class DynamicEventRuntimePopup : UIView<DynamicEventRuntimePresenter>
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _narrationText;
        [SerializeField] private TextMeshProUGUI _choicesText;
        [SerializeField] private RectTransform _choiceButtonRoot;
        [SerializeField] private Button _pauseForManualButton;

        internal GameObject PanelRoot => _panelRoot;
        internal TextMeshProUGUI TitleText => _titleText;
        internal TextMeshProUGUI NarrationText => _narrationText;
        internal TextMeshProUGUI ChoicesText => _choicesText;
        internal RectTransform ChoiceButtonRoot => _choiceButtonRoot;
        internal Button PauseForManualButton => _pauseForManualButton;

        protected override void Awake()
        {
            base.Awake();
            HidePanel();
            if (_pauseForManualButton != null)
                _pauseForManualButton.onClick.AddListener(() => Presenter?.OnPauseForManualClicked());
        }

        private void Start()
        {
            Presenter?.OnOpen();
        }

        private void OnDestroy()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Presenter?.OnClose();
        }

        /// <summary>패널 루트를 숨긴다.</summary>
        internal void HidePanel()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        /// <summary>패널 루트를 표시한다.</summary>
        internal void ShowPanel()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
        }

        /// <summary>선택지 버튼을 모두 제거한다.</summary>
        internal void ClearChoiceButtons()
        {
            if (_choiceButtonRoot == null)
                return;

            for (var i = _choiceButtonRoot.childCount - 1; i >= 0; i--)
                Destroy(_choiceButtonRoot.GetChild(i).gameObject);
        }

        /// <summary>선택지 버튼을 생성한다.</summary>
        internal Button CreateChoiceButton(int number, string label)
        {
            var font = RuntimeUiTmpFont.Get();
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

            var text = labelGo.AddComponent<TextMeshProUGUI>();
            UiTmpUtil.ApplyButtonLabel(text, font, 15, TextAnchor.MiddleCenter);
            text.color = Color.white;
            text.text = $"<b>{number}.</b> {label}";
            return button;
        }

        /// <summary>Golden/Standard 프레젠테이션 스타일을 적용한다.</summary>
        internal void ApplyPresentation(bool isSpecial)
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
    }

    public sealed class DynamicEventRuntimePresenter : UIPresenter<DynamicEventRuntimePopup>
    {
        private DynamicEventInstance _currentInstance;
        private CompositeDisposable _disposables;

        public override void OnOpen()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            DynamicEventChannels.OnEventSceneReady
                .Subscribe(ShowScene)
                .AddTo(_disposables);

            DynamicEventChannels.OnEventResolved
                .Subscribe(_ => Hide())
                .AddTo(_disposables);
        }

        public override void OnClose()
        {
            _disposables?.Dispose();
            _disposables = null;
            _currentInstance = null;
        }

        /// <summary>수동 선택 모드 진입 후 UI를 갱신한다.</summary>
        public void OnPauseForManualClicked()
        {
            if (!DynamicEventSystem.TryEnterManualChoiceMode())
                return;

            if (_currentInstance != null)
                ShowScene(_currentInstance);
        }

        private void ShowScene(DynamicEventInstance instance)
        {
            if (instance?.LlmNarration == null || View.PanelRoot == null || View.TitleText == null)
                return;

            _currentInstance = instance;
            View.ApplyPresentation(instance.Intensity == DynamicEventIntensity.Golden);

            View.TitleText.text = instance.Intensity == DynamicEventIntensity.Golden
                ? $"<color=#ffd966>[★ 황금 이벤트]</color> {instance.TemplateId} · {instance.Floor}층"
                : $"[이벤트] {instance.TemplateId} · {instance.Floor}층";
            View.NarrationText.text = instance.LlmNarration.Narration;

            View.ChoicesText.text = instance.RequiresManualChoice
                ? "<b>선택지 (버튼 또는 1/2 키)</b>"
                : "<b>자동 진행 중 · 원하면 직접 선택</b>";

            RebuildChoiceButtons(instance);
            if (View.PauseForManualButton != null)
                View.PauseForManualButton.gameObject.SetActive(!instance.RequiresManualChoice);
            View.ShowPanel();
        }

        private void RebuildChoiceButtons(DynamicEventInstance instance)
        {
            View.ClearChoiceButtons();

            var choices = instance.LlmNarration.Choices;
            if (choices == null || View.ChoiceButtonRoot == null)
                return;

            for (var i = 0; i < choices.Count; i++)
            {
                var choiceIndex = i;
                var choice = choices[i];
                var button = View.CreateChoiceButton(choiceIndex + 1, choice.Text);
                button.onClick.AddListener(() => DynamicEventSystem.TrySubmitManualChoice(choiceIndex));
            }
        }

        private void Hide()
        {
            _currentInstance = null;
            View.HidePanel();
        }
    }
}
