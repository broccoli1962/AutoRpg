using Backend.GameSystems.DynamicEvent.Data;
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
        private GameObject _panelRoot;
        private Text _titleText;
        private Text _narrationText;
        private Text _choicesText;
        private RectTransform _choiceButtonRoot;
        private Button _pauseForManualButton;
        private DynamicEventInstance _currentInstance;
        private CompositeDisposable _disposables;

        private void Start()
        {
            BuildUi();
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

        private void BuildUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _panelRoot = new GameObject("DynamicEventPopup");
            _panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var dim = _panelRoot.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            dim.raycastTarget = true;

            var box = new GameObject("Box");
            box.transform.SetParent(_panelRoot.transform, false);
            var boxRect = box.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(720f, 420f);

            var boxImage = box.AddComponent<Image>();
            boxImage.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

            _titleText = CreateText(box.transform, "Title", new Vector2(24f, -20f), 24, "[이벤트]");
            _narrationText = CreateText(box.transform, "Narration", new Vector2(24f, -60f), 18, string.Empty);
            _choicesText = CreateText(box.transform, "Choices", new Vector2(24f, -240f), 16, string.Empty);

            var narrationRect = _narrationText.rectTransform;
            narrationRect.sizeDelta = new Vector2(672f, 160f);
            _narrationText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _narrationText.verticalOverflow = VerticalWrapMode.Overflow;

            var choicesRect = _choicesText.rectTransform;
            choicesRect.sizeDelta = new Vector2(672f, 48f);
            _choicesText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _choicesText.verticalOverflow = VerticalWrapMode.Overflow;
            _choicesText.color = new Color(0.85f, 0.9f, 1f);

            var choiceRootGo = new GameObject("ChoiceButtons");
            choiceRootGo.transform.SetParent(box.transform, false);
            _choiceButtonRoot = choiceRootGo.AddComponent<RectTransform>();
            _choiceButtonRoot.anchorMin = new Vector2(0f, 0f);
            _choiceButtonRoot.anchorMax = new Vector2(1f, 0f);
            _choiceButtonRoot.pivot = new Vector2(0.5f, 0f);
            _choiceButtonRoot.anchoredPosition = new Vector2(0f, 24f);
            _choiceButtonRoot.sizeDelta = new Vector2(-48f, 96f);

            var layout = choiceRootGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(24, 24, 0, 0);

            _pauseForManualButton = CreateActionButton(
                box.transform,
                "PauseForManualButton",
                new Vector2(24f, 132f),
                new Vector2(220f, 36f),
                "일시정지하고 직접 선택");
            _pauseForManualButton.onClick.AddListener(OnPauseForManualClicked);
        }

        private void ShowScene(DynamicEventInstance instance)
        {
            if (instance?.LlmNarration == null)
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
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            var labelRect = labelGo.AddComponent<RectTransform>();
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

        private static Button CreateActionButton(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            Vector2 size,
            string label)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.24f, 0.18f, 0.12f, 0.95f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var text = labelGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 13;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.92f, 0.75f);
            text.text = label;
            return button;
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
            rect.sizeDelta = new Vector2(672f, 40f);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;
            text.text = initial;
            return text;
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
