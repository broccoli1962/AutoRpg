using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Prestige.Data;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 캐릭터 전직(Tier) — 견습~전설 (07_캐릭터시스템.md).
    /// </summary>
    public static class CharacterTierSystem
    {
        public const int MaxTierIndex = (int)CharacterTier.Legend;

        public static int GetTierIndex(string characterId)
        {
            if (GameStateUtil.IsQuitting || string.IsNullOrEmpty(characterId))
                return 0;

            var meta = PrestigeManager.GetMeta();
            if (meta?.CharacterTiers == null)
                return 0;

            foreach (var record in meta.CharacterTiers)
            {
                if (record.CharacterId == characterId)
                    return Mathf.Clamp(record.TierIndex, 0, MaxTierIndex);
            }

            return 0;
        }

        public static CharacterTier GetTier(string characterId) =>
            (CharacterTier)GetTierIndex(characterId);

        public static string GetTierLabel(CharacterTier tier) =>
            tier switch
            {
                CharacterTier.Adept => "숙련자",
                CharacterTier.Artisan => "장인",
                CharacterTier.Hero => "영웅",
                CharacterTier.Legend => "전설",
                _ => "견습"
            };

        public static string GetTierTitle(string characterId) =>
            GetTierLabel(GetTier(characterId));

        public static (int reputation, int legacy, int relic) GetPromoteCost(int targetTierIndex) =>
            targetTierIndex switch
            {
                1 => (3, 2, 0),
                2 => (8, 6, 0),
                3 => (15, 12, 2),
                4 => (25, 24, 5),
                _ => (0, 0, 0)
            };

        public static void ApplyPartyTiers(PartyState party)
        {
            if (party?.Members == null)
                return;

            foreach (var member in party.Members)
            {
                if (member == null)
                    continue;

                var tier = GetTierIndex(member.CharacterId);
                if (tier <= 0)
                    continue;

                member.Str += tier;
                member.Agi += tier;
                member.Int += tier;
                member.Vit += tier;
                member.Luk += tier / 2;
                member.MaxHp += tier * 6;
                member.CurrentHp = member.MaxHp;
            }
        }

        public static bool TryPromote(string characterId, out string message)
        {
            message = null;
            if (GameStateUtil.IsQuitting || string.IsNullOrEmpty(characterId))
            {
                message = "캐릭터 없음";
                return false;
            }

            var meta = PrestigeManager.GetMeta();
            if (meta?.CharacterTiers == null)
            {
                message = "메타 데이터 없음";
                return false;
            }

            var current = GetTierIndex(characterId);
            if (current >= MaxTierIndex)
            {
                message = "최대 전직";
                return false;
            }

            var next = current + 1;
            var cost = GetPromoteCost(next);
            if (meta.Reputation < cost.reputation ||
                meta.LegacyPoints < cost.legacy ||
                meta.RelicFragments < cost.relic)
            {
                message = $"명성 {cost.reputation} · 유산 {cost.legacy} · 유물 {cost.relic} 필요";
                return false;
            }

            meta.Reputation -= cost.reputation;
            meta.LegacyPoints -= cost.legacy;
            meta.RelicFragments -= cost.relic;

            CharacterTierRecord record = null;
            foreach (var entry in meta.CharacterTiers)
            {
                if (entry.CharacterId != characterId)
                    continue;

                record = entry;
                break;
            }

            if (record == null)
            {
                record = new CharacterTierRecord { CharacterId = characterId };
                meta.CharacterTiers.Add(record);
            }

            record.TierIndex = next;
            message = $"{GetTierLabel((CharacterTier)next)} 전직 완료";
            Debug.Log($"[CharacterTierSystem] {characterId} promoted to tier {next}");
            GameSaveManager.Save();
            return true;
        }

        public static bool TryPromoteLeader(out string message)
        {
            var leader = ExplorationSystem.GetCurrentState()?.Party?.Leader;
            if (leader == null)
            {
                message = "파티 리더 없음";
                return false;
            }

            return TryPromote(leader.CharacterId, out message);
        }

        public static string GetDisplayLabel(string characterId)
        {
            var tier = GetTier(characterId);
            if (tier >= CharacterTier.Legend)
                return $"{GetTierLabel(tier)}:MAX";

            var next = GetPromoteCost((int)tier + 1);
            return $"{GetTierLabel(tier)} (다음 명성{next.reputation}/유산{next.legacy})";
        }
    }
}
