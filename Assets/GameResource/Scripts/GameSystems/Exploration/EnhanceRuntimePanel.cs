using Backend.GameSystems.Equipment;
using Backend.GameSystems.Equipment.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using UnityEngine;
using Backend.Util;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭 '강화/장비' — 전직·장비 강화 (09_성장과경제.md).
    /// </summary>
    public sealed class EnhanceRuntimePanel : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Text _contentText;
        private bool _isVisible;

        public bool IsVisible => _isVisible;

        private void Start()
        {
            BuildUi();
            Hide();
        }

        private void Update()
        {
            if (!_isVisible)
                return;

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
            {
                CharacterTierManager.TryPromoteLeader(out var message);
                Debug.Log($"[EnhancePanel] {message}");
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
            {
                EquipmentEnhanceManager.TryEnhanceLeaderWeapon(out var message);
                Debug.Log($"[EnhancePanel] {message}");
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
            {
                EquipmentEnhanceManager.TryEnhanceLeaderArmor(out var message);
                Debug.Log($"[EnhancePanel] {message}");
                RefreshContent();
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

            _panelRoot = new GameObject("EnhancePanel");
            _panelRoot.transform.SetParent(canvas.transform, false);

            var panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(640f, 420f);

            var panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.09f, 0.12f, 0.96f);

            var title = CreateText(_panelRoot.transform, "Title", new Vector2(20f, -16f), 22, "[ 강화 / 장비 ]");
            title.rectTransform.sizeDelta = new Vector2(600f, 32f);

            var hint = CreateText(_panelRoot.transform, "Hint", new Vector2(20f, -44f), 13,
                "1:전직  2:무기 강화  3:방어구 강화  (리더 기준 · 대장간 Lv.1+)");
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

            var leader = ExplorationManager.GetCurrentState()?.Party?.Leader;
            if (leader == null)
            {
                _contentText.text = "탐험 중인 파티가 없습니다.";
                return;
            }

            var meta = PrestigeManager.GetMeta();
            var weaponLevel = EquipmentEnhanceManager.GetEnhanceLevel(leader.CharacterId, EquipmentSlot.Weapon);
            var armorLevel = EquipmentEnhanceManager.GetEnhanceLevel(leader.CharacterId, EquipmentSlot.Armor);
            var maxEnhance = EquipmentEnhanceManager.GetMaxEnhanceLevel();
            var nextWeapon = EquipmentEnhanceManager.GetEnhanceCost(weaponLevel + 1);
            var nextArmor = EquipmentEnhanceManager.GetEnhanceCost(armorLevel + 1);
            var nextTier = CharacterTierManager.GetPromoteCost(CharacterTierManager.GetTierIndex(leader.CharacterId) + 1);

            _contentText.text =
                $"<b>{leader.DisplayName}</b> · {CharacterTierManager.GetDisplayLabel(leader.CharacterId)}\n\n" +
                $"<b>장비</b>\n{EquipmentService.GetMemberEquipmentSummary(leader) ?? "장비 없음"}\n" +
                $"무기 +{weaponLevel}/{maxEnhance} · 방어구 +{armorLevel}/{maxEnhance}\n" +
                $"다음 강화 비용 — 무기: 유산 {nextWeapon.legacy}/마나 {nextWeapon.mana} · " +
                $"방어구: 유산 {nextArmor.legacy}/마나 {nextArmor.mana}\n\n" +
                $"<b>전직 비용 (다음)</b>\n명성 {nextTier.reputation} · 유산 {nextTier.legacy} · 유물 {nextTier.relic}\n\n" +
                $"<b>보유 자원</b>\n명성 {meta?.Reputation ?? 0} · 유산 {meta?.LegacyPoints ?? 0} · " +
                $"마나 {meta?.ManaShards ?? 0} · 유물 {meta?.RelicFragments ?? 0}\n" +
                $"대장간 {BlacksmithManager.GetDisplayLabel()}";
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
