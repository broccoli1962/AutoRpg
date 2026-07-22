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
    public sealed class EnhanceRuntimePanel : ExplorationOverlayView
    {
        [SerializeField] private Text _contentText;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!IsVisible)
                return;

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
            {
                CharacterTierManager.TryPromoteLeader(out var message);
                Debug.Log($"[EnhanceRuntimePanel] {message}");
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
            {
                EquipmentEnhanceManager.TryEnhanceLeaderWeapon(out var message);
                Debug.Log($"[EnhanceRuntimePanel] {message}");
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
            {
                EquipmentEnhanceManager.TryEnhanceLeaderArmor(out var message);
                Debug.Log($"[EnhanceRuntimePanel] {message}");
                RefreshContent();
            }
        }

        protected override void OnBeforeShow()
        {
            RefreshContent();
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
    }
}
