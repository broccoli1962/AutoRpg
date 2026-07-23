using Backend.GameSystems.Equipment;
using Backend.GameSystems.Equipment.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 대장간 — 장비 드롭·시작 장비 보너스 (09_성장과경제.md).
    /// </summary>
    public static class BlacksmithSystem
    {
        public const int MaxLevel = 3;

        public static int Level
        {
            get
            {
                if (GameStateUtil.IsQuitting)
                    return 0;

                return PrestigeManager.GetMeta()?.BlacksmithLevel ?? 0;
            }
        }

        public static float GetDropChanceMultiplier() => 1f + Level * 0.12f;

        public static (int legacy, int mana) GetUpgradeCost(int targetLevel) =>
            targetLevel switch
            {
                1 => (4, 2),
                2 => (10, 6),
                3 => (20, 12),
                _ => (0, 0)
            };

        public static void ApplyStartingEquipment(PartyState party)
        {
            if (party?.Members == null || Level < 2)
                return;

            foreach (var member in party.Members)
            {
                if (member == null || member.CurrentHp <= 0)
                    continue;

                if (member.Role == CharacterRole.Warrior || member.Role == CharacterRole.Rogue)
                {
                    if (string.IsNullOrEmpty(member.EquippedWeaponId))
                        member.EquippedWeaponId = EquipmentDefinitions.RustyBladeId;
                }

                if ((member.Role == CharacterRole.Warrior || member.Role == CharacterRole.Cleric) &&
                    string.IsNullOrEmpty(member.EquippedArmorId))
                    member.EquippedArmorId = EquipmentDefinitions.MossLeatherId;
            }
        }

        public static bool CanUpgrade(out string reason)
        {
            reason = null;
            if (GameStateUtil.IsQuitting)
            {
                reason = "종료 중";
                return false;
            }

            var meta = PrestigeManager.GetMeta();
            if (meta == null)
            {
                reason = "메타 데이터 없음";
                return false;
            }

            if (meta.BlacksmithLevel >= MaxLevel)
            {
                reason = "최대 레벨";
                return false;
            }

            var cost = GetUpgradeCost(meta.BlacksmithLevel + 1);
            if (meta.LegacyPoints < cost.legacy || meta.ManaShards < cost.mana)
            {
                reason = $"유산 {cost.legacy} · 마나 {cost.mana} 필요";
                return false;
            }

            return true;
        }

        public static bool TryUpgrade(out string message)
        {
            if (!CanUpgrade(out message))
                return false;

            var meta = PrestigeManager.GetMeta();
            var nextLevel = meta.BlacksmithLevel + 1;
            var cost = GetUpgradeCost(nextLevel);
            meta.LegacyPoints -= cost.legacy;
            meta.ManaShards -= cost.mana;
            meta.BlacksmithLevel = nextLevel;
            message = $"대장간 Lv.{nextLevel} 해금";
            Debug.Log($"[BlacksmithSystem] Upgraded to level {nextLevel}");
            GameSaveManager.Save();
            return true;
        }

        public static string GetDisplayLabel()
        {
            if (Level >= MaxLevel)
                return "대장간:MAX";

            var next = GetUpgradeCost(Level + 1);
            return $"대장간:Lv.{Level} (다음 유산{next.legacy}/마나{next.mana})";
        }

        public static string GetBonusSummary() =>
            Level switch
            {
                0 => "보너스 없음",
                1 => "장비 드롭 +12%",
                2 => "드롭 +24% · 시작 무기/갑옷",
                _ => "드롭 +36% · 시작 무기/갑옷"
            };
    }
}
