using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.LLM;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 탐험 설정 런타임 패널 (12_UIUX.md LLM/이벤트 옵션).
    /// </summary>
    public sealed class ExplorationSettingsRuntimePanel : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Text _contentText;
        private bool _isVisible;
        private System.Action _onSettingsChanged;

        public bool IsVisible => _isVisible;

        public void Configure(System.Action onSettingsChanged)
        {
            _onSettingsChanged = onSettingsChanged;
        }

        private void Start()
        {
            BuildUi();
            Hide();
        }

        private void Update()
        {
            if (!_isVisible)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                LlmQualitySettings.CycleMode();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                DynamicEventAutoPolicySettings.CyclePolicy();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                GoldenEventSettings.ToggleAutoPause();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                LogFrequencySettings.CycleMode();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                OfflineSummaryDetailSettings.ToggleMode();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                if (ScriptoriumManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettings] {message}");
                else
                    Debug.Log($"[ExplorationSettings] 서고 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            {
                if (TrainingGroundManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettings] {message}");
                else
                    Debug.Log($"[ExplorationSettings] 훈련소 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                if (BlacksmithManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettings] {message}");
                else
                    Debug.Log($"[ExplorationSettings] 대장간 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
            {
                if (InnManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettings] {message}");
                else
                    Debug.Log($"[ExplorationSettings] 여관 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                if (BookshopManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettings] {message}");
                else
                    Debug.Log($"[ExplorationSettings] 서점 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }
        }

        public void Toggle()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        private void BuildUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _panelRoot = new GameObject("SettingsPanel");
            _panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(640f, 440f);

            var panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.14f, 0.96f);

            var title = CreateText(_panelRoot.transform, "Title", new Vector2(20f, -16f), 22,
                "[ 탐험 설정 ]");
            title.rectTransform.sizeDelta = new Vector2(600f, 32f);

            var hint = CreateText(_panelRoot.transform, "Hint", new Vector2(20f, -44f), 13,
                "1:LLM  2:이벤트  3:황금  4:로그  5:오프라인  6:서고  7:훈련  8:대장  9:여관  0:서점  (O:닫기)");
            hint.rectTransform.sizeDelta = new Vector2(600f, 20f);
            hint.color = new Color(0.75f, 0.75f, 0.8f);

            _contentText = CreateText(_panelRoot.transform, "Content", new Vector2(20f, -68f), 16, string.Empty);
            _contentText.rectTransform.sizeDelta = new Vector2(600f, 308f);
            _contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _contentText.verticalOverflow = VerticalWrapMode.Overflow;
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
            rect.sizeDelta = new Vector2(600f, 40f);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;
            text.text = initial;
            return text;
        }

        private void Show()
        {
            RefreshContent();
            _panelRoot.SetActive(true);
            _isVisible = true;
        }

        private void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            _isVisible = false;
        }

        private void RefreshContent()
        {
            if (_contentText == null)
                return;

            _contentText.text =
                $"<b>1. LLM 텍스트 품질</b>\n{LlmQualitySettings.GetDisplayLabel()}\n\n" +
                $"<b>2. 자동 이벤트 선택 정책</b>\n{DynamicEventAutoPolicySettings.GetDisplayLabel()}\n\n" +
                $"<b>3. 황금 이벤트 자동정지</b>\n{GoldenEventSettings.GetDisplayLabel()}\n\n" +
                $"<b>4. 로그 생성 빈도</b>\n{LogFrequencySettings.GetDisplayLabel()} · 최소 {GetSalienceLabel(LogFrequencySettings.GetMinimumSalience())}\n\n" +
                $"<b>5. 오프라인 요약 상세도</b>\n{OfflineSummaryDetailSettings.GetDisplayLabel()}\n\n" +
                $"<b>6. 필사가의 서고</b>\n{ScriptoriumManager.GetDisplayLabel()}\n{ScriptoriumManager.GetBonusSummary()}\n\n" +
                $"<b>7. 훈련소</b>\n{TrainingGroundManager.GetDisplayLabel()}\n{TrainingGroundManager.GetBonusSummary()}\n\n" +
                $"<b>8. 대장간</b>\n{BlacksmithManager.GetDisplayLabel()}\n{BlacksmithManager.GetBonusSummary()}\n\n" +
                $"<b>9. 여관</b>\n{InnManager.GetDisplayLabel()}\n{InnManager.GetBonusSummary()}\n\n" +
                $"<b>0. 서점</b>\n{BookshopManager.GetDisplayLabel()}\n{BookshopManager.GetBonusSummary()}";
        }

        private static string GetSalienceLabel(SalienceGrade grade)
        {
            return grade switch
            {
                SalienceGrade.Significant => "Significant+",
                SalienceGrade.Trivial => "전체",
                _ => "Notable+"
            };
        }
    }
}
