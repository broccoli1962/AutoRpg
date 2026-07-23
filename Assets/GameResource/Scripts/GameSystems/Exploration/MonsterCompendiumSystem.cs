using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 전투 승리로 해금되는 몬스터 도감 (12_UIUX.md 연대기 하위 탭).
    /// </summary>
    public static class MonsterCompendiumSystem
    {
        public static IReadOnlyList<string> GetEntries()
        {
            if (GameStateUtil.IsQuitting)
                return System.Array.Empty<string>();

            var entries = PrestigeManager.GetMeta()?.MonsterEntries;
            return entries != null ? entries : System.Array.Empty<string>();
        }

        public static void RecordCombat(ExplorationEvent explorationEvent)
        {
            if (explorationEvent?.Combat == null ||
                explorationEvent.Combat.Outcome != CombatOutcome.Victory ||
                explorationEvent.Combat.EnemyGroup == null ||
                explorationEvent.Combat.EnemyGroup.Count == 0)
            {
                return;
            }

            var monsterId = explorationEvent.Combat.EnemyGroup[0];
            if (string.IsNullOrEmpty(monsterId))
                return;

            var displayName = string.IsNullOrEmpty(explorationEvent.Combat.MonsterDisplayName)
                ? monsterId
                : explorationEvent.Combat.MonsterDisplayName;

            var zoneName = ZoneDefinitions.GetZoneDisplayName(explorationEvent.ZoneId);
            var rarity = ResolveRarity(explorationEvent.ZoneId, monsterId);
            var entry = $"{zoneName} | {displayName} ({FormatRarity(rarity)})";
            var key = BuildKey(explorationEvent.ZoneId, monsterId);
            TryAdd(key, entry);
        }

        private static MonsterRarity ResolveRarity(string zoneId, string monsterId)
        {
            foreach (var monster in ZoneDefinitions.GetMonsters(zoneId))
            {
                if (monster.Id == monsterId)
                    return monster.Rarity;
            }

            return MonsterRarity.Common;
        }

        private static string BuildKey(string zoneId, string monsterId) => $"{zoneId}:{monsterId}";

        private static string FormatRarity(MonsterRarity rarity) =>
            rarity switch
            {
                MonsterRarity.Notable => "주목",
                MonsterRarity.Rare => "희귀",
                MonsterRarity.Boss => "보스",
                _ => "일반"
            };

        private static void TryAdd(string key, string entry)
        {
            if (GameStateUtil.IsQuitting || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(entry))
                return;

            var meta = PrestigeManager.GetMeta();
            if (meta?.MonsterEntries == null)
                return;

            foreach (var existing in meta.MonsterEntries)
            {
                if (existing.StartsWith(key + "|"))
                    return;
            }

            meta.MonsterEntries.Add($"{key}|{entry}");
            GameSaveManager.Save();
        }
    }
}
