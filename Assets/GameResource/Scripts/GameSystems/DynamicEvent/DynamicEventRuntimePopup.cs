using Backend.GameSystems.DynamicEvent.Data;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.DynamicEvent
{
    /// <summary>
    /// Phase 4 동적 이벤트 Standard 강도 런타임 팝업. LLM 연출 장면과 선택지를 표시한다.
    /// </summary>
    public sealed class DynamicEventRuntimePopup : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Text _titleText;
        private Text _narrationText;
        private Text _choicesText;
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
            choicesRect.sizeDelta = new Vector2(672f, 120f);
            _choicesText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _choicesText.verticalOverflow = VerticalWrapMode.Overflow;
            _choicesText.color = new Color(0.85f, 0.9f, 1f);
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

        private void ShowScene(DynamicEventInstance instance)
        {
            if (instance?.LlmNarration == null)
                return;

            _titleText.text = instance.RequiresManualChoice
                ? $"<color=#ffd966>[★ 황금 이벤트]</color> {instance.TemplateId} · {instance.Floor}층"
                : $"[이벤트] {instance.TemplateId} · {instance.Floor}층";
            _narrationText.text = instance.LlmNarration.Narration;

            var builder = new System.Text.StringBuilder();
            builder.AppendLine(instance.RequiresManualChoice
                ? "<b>선택지 (1/2 키로 직접 선택)</b>"
                : "<b>선택지 (자동 진행)</b>");
            var index = 1;
            foreach (var choice in instance.LlmNarration.Choices)
            {
                if (instance.RequiresManualChoice)
                {
                    builder.Append(index);
                    builder.Append(". ");
                    index++;
                }
                else
                {
                    builder.Append("• ");
                }

                builder.Append(choice.Text);
                builder.Append(" (");
                builder.Append(choice.Id);
                builder.AppendLine(")");
            }

            _choicesText.text = builder.ToString();
            _panelRoot.SetActive(true);
        }

        private void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }
    }
}
