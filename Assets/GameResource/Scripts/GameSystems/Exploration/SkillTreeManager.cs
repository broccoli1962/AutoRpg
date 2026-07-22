using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 역할별 스킬 트리 Lv.1~3 — 유산·마나로 패시브 해금 (07_캐릭터시스템.md).
    /// </summary>
    public static class SkillTreeManager
    {
        public const int MaxTier = 3;
        private const int BardTokenBonusPerTier = 8;
        private const float RogueCritBonusPerTier = 0.04f;

        public static int GetTier(CharacterRole role)
        {
            if (GameStateUtil.IsQuitting)
                return 0;

            var meta = PrestigeManager.GetMeta();
            if (meta?.UnlockedSkillIds == null)
                return 0;

            var tier = 0;
            for (var i = 1; i <= MaxTier; i++)
            {
                if (meta.UnlockedSkillIds.Contains(BuildSkillId(role, i)))
                    tier = i;
            }

            return tier;
        }

        public static (int legacy, int mana, int reputation) GetUpgradeCost(CharacterRole role, int targetTier) =>
            targetTier switch
            {
                1 => (5, 3, 0),
                2 => (12, 8, 0),
                3 => (24, 15, 5),
                _ => (0, 0, 0)
            };

        public static void ApplyPartyBonuses(PartyState party)
        {
            if (party?.Members == null)
                return;

            foreach (var member in party.Members)
            {
                if (member == null)
                    continue;

                ApplyRoleBonuses(member);
            }
        }

        public static int GetBardTokenBonus(PartyState party)
        {
            var bonus = 0;
            if (party?.Members == null)
                return bonus;

            foreach (var member in party.Members)
            {
                if (member?.Role != CharacterRole.Bard)
                    continue;

                bonus += GetTier(CharacterRole.Bard) * BardTokenBonusPerTier;
            }

            return bonus;
        }

        public static float GetRogueCritChanceBonus(CharacterState attacker)
        {
            if (attacker?.Role != CharacterRole.Rogue)
                return 0f;

            return GetTier(CharacterRole.Rogue) * RogueCritBonusPerTier;
        }

        public static float GetClericRestHealBonus(PartyState party)
        {
            if (party?.Members == null)
                return 1f;

            foreach (var member in party.Members)
            {
                if (member?.Role == CharacterRole.Cleric && GetTier(CharacterRole.Cleric) >= 3)
                    return 1.15f;
            }

            return 1f;
        }

        public static bool TryUpgrade(CharacterRole role, out string message)
        {
            message = null;
            if (GameStateUtil.IsQuitting)
            {
                message = "종료 중";
                return false;
            }

            var meta = PrestigeManager.GetMeta();
            if (meta?.UnlockedSkillIds == null)
            {
                message = "메타 데이터 없음";
                return false;
            }

            var currentTier = GetTier(role);
            if (currentTier >= MaxTier)
            {
                message = $"{GetRoleLabel(role)} 스킬 MAX";
                return false;
            }

            var nextTier = currentTier + 1;
            var cost = GetUpgradeCost(role, nextTier);
            if (meta.LegacyPoints < cost.legacy ||
                meta.ManaShards < cost.mana ||
                meta.Reputation < cost.reputation)
            {
                message = $"유산 {cost.legacy} · 마나 {cost.mana} · 명성 {cost.reputation} 필요";
                return false;
            }

            meta.LegacyPoints -= cost.legacy;
            meta.ManaShards -= cost.mana;
            meta.Reputation -= cost.reputation;
            meta.UnlockedSkillIds.Add(BuildSkillId(role, nextTier));
            message = $"{GetRoleLabel(role)} 스킬 T{nextTier} 해금";
            Debug.Log($"[SkillTreeManager] Unlocked {BuildSkillId(role, nextTier)}");
            GameSaveManager.Save();
            return true;
        }

        public static bool TryUpgradeLeaderRole(out string message)
        {
            var leader = ExplorationManager.GetCurrentState()?.Party?.Leader;
            if (leader == null)
            {
                message = "파티 리더 없음";
                return false;
            }

            return TryUpgrade(leader.Role, out message);
        }

        public static string GetDisplayLabel(CharacterRole role)
        {
            var tier = GetTier(role);
            if (tier >= MaxTier)
                return $"{GetRoleLabel(role)}:TMAX";

            var next = GetUpgradeCost(role, tier + 1);
            return $"{GetRoleLabel(role)}:T{tier} (다음 유산{next.legacy}/마나{next.mana})";
        }

        public static string GetLeaderDisplayLabel()
        {
            var leader = ExplorationManager.GetCurrentState()?.Party?.Leader;
            return leader == null ? "스킬:- " : GetDisplayLabel(leader.Role);
        }

        private static void ApplyRoleBonuses(CharacterState member)
        {
            var tier = GetTier(member.Role);
            if (tier <= 0)
                return;

            switch (member.Role)
            {
                case CharacterRole.Warrior:
                    member.Str += 2 * tier;
                    member.Vit += tier;
                    break;
                case CharacterRole.Rogue:
                    member.Agi += 2 * tier;
                    member.Str += tier;
                    break;
                case CharacterRole.Mage:
                    member.Int += 2 * tier;
                    break;
                case CharacterRole.Bard:
                    member.Int += tier;
                    member.Luk += 2 * tier;
                    break;
                case CharacterRole.Cleric:
                    member.Int += tier;
                    member.Vit += 2 * tier;
                    break;
            }

            member.MaxHp += tier * 8;
            member.CurrentHp = member.MaxHp;
        }

        private static string BuildSkillId(CharacterRole role, int tier) => $"{role.ToString().ToLowerInvariant()}_t{tier}";

        private static string GetRoleLabel(CharacterRole role) =>
            role switch
            {
                CharacterRole.Warrior => "전사",
                CharacterRole.Rogue => "도적",
                CharacterRole.Mage => "마법사",
                CharacterRole.Bard => "음유시인",
                CharacterRole.Cleric => "성직자",
                _ => role.ToString()
            };
    }
}
