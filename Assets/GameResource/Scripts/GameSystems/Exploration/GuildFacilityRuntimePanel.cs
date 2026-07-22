using UnityEngine;
using Backend.Util;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭 '길드시설' — 시설 레벨 요약 및 업그레이드 (12_UIUX.md).
    /// </summary>
    public sealed class GuildFacilityRuntimePanel : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Text _contentText;
        private bool _isVisible;
        private System.Action _onChanged;

        public bool IsVisible => _isVisible;

        public void Configure(System.Action onChanged)
        {
            _onChanged = onChanged;
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

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha6, KeyCode.Keypad6))
            {
                ScriptoriumManager.TryUpgrade(out var message);
                Debug.Log($"[GuildPanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha7, KeyCode.Keypad7))
            {
                TrainingGroundManager.TryUpgrade(out var message);
                Debug.Log($"[GuildPanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha8, KeyCode.Keypad8))
            {
                BlacksmithManager.TryUpgrade(out var message);
                Debug.Log($"[GuildPanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha9, KeyCode.Keypad9))
            {
                InnManager.TryUpgrade(out var message);
                Debug.Log($"[GuildPanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha0, KeyCode.Keypad0))
            {
                BookshopManager.TryUpgrade(out var message);
                Debug.Log($"[GuildPanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Minus, KeyCode.KeypadMinus))
            {
                SkillTreeManager.TryUpgradeLeaderRole(out var message);
                Debug.Log($"[GuildPanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }
        }

        public void Show()
        {
            RefreshContent();
            _panelRoot.SetActive(true);
            _isVisible = true;
        }

        public void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            _isVisible = false;
        }

        private void BuildUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _panelRoot = new GameObject("GuildFacilityPanel");
            _panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(640f, 420f);

            var panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.14f, 0.96f);

            var title = CreateText(_panelRoot.transform, "Title", new Vector2(20f, -16f), 22, "[ 길드 시설 ]");
            title.rectTransform.sizeDelta = new Vector2(600f, 32f);

            var hint = CreateText(_panelRoot.transform, "Hint", new Vector2(20f, -44f), 13,
                "6:서고  7:훈련  8:대장  9:여관  0:서점  -:스킬  (O:탐험 설정)");
            hint.rectTransform.sizeDelta = new Vector2(600f, 20f);
            hint.color = new Color(0.75f, 0.75f, 0.8f);

            _contentText = CreateText(_panelRoot.transform, "Content", new Vector2(20f, -68f), 16, string.Empty);
            _contentText.rectTransform.sizeDelta = new Vector2(600f, 300f);
            _contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _contentText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void RefreshContent()
        {
            if (_contentText == null)
                return;

            _contentText.text =
                $"<b>6. 필사가의 서고</b>\n{ScriptoriumManager.GetDisplayLabel()}\n{ScriptoriumManager.GetBonusSummary()}\n\n" +
                $"<b>7. 훈련소</b>\n{TrainingGroundManager.GetDisplayLabel()}\n{TrainingGroundManager.GetBonusSummary()}\n\n" +
                $"<b>8. 대장간</b>\n{BlacksmithManager.GetDisplayLabel()}\n{BlacksmithManager.GetBonusSummary()}\n\n" +
                $"<b>9. 여관</b>\n{InnManager.GetDisplayLabel()}\n{InnManager.GetBonusSummary()}\n\n" +
                $"<b>0. 서점</b>\n{BookshopManager.GetDisplayLabel()}\n{BookshopManager.GetBonusSummary()}\n\n" +
                $"<b>-. 스킬 트리 (리더)</b>\n{SkillTreeManager.GetLeaderDisplayLabel()}";
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
    }
}
