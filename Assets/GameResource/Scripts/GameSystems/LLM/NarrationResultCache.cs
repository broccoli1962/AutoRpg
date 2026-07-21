using System;
using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// 유사 이벤트 컨텍스트의 LLM 생성 결과를 캐시하여 재사용한다. (기획서 10장 캐싱 전략)
    /// </summary>
    public static class NarrationResultCache
    {
        private const float ReuseProbability = 0.4f;
        private const int MaxEntriesPerKey = 32;

        private static readonly Dictionary<string, List<string>> Pool = new();
        private static readonly Random Random = new();

        public static bool TryGetCached(ExplorationEvent explorationEvent, PartyState party, out string text)
        {
            text = null;
            var key = BuildCacheKey(explorationEvent, party);
            if (!Pool.TryGetValue(key, out var entries) || entries.Count == 0)
                return false;

            if (Random.NextDouble() > ReuseProbability)
                return false;

            text = entries[Random.Next(entries.Count)];
            return !string.IsNullOrWhiteSpace(text);
        }

        public static void Store(ExplorationEvent explorationEvent, PartyState party, string generatedText)
        {
            if (string.IsNullOrWhiteSpace(generatedText))
                return;

            var key = BuildCacheKey(explorationEvent, party);
            if (!Pool.TryGetValue(key, out var entries))
            {
                entries = new List<string>();
                Pool[key] = entries;
            }

            entries.Add(generatedText.Trim());
            if (entries.Count > MaxEntriesPerKey)
                entries.RemoveAt(0);
        }

        private static string BuildCacheKey(ExplorationEvent explorationEvent, PartyState party)
        {
            var leader = party?.Leader;
            var personality = leader?.PersonalityTags != null && leader.PersonalityTags.Count > 0
                ? leader.PersonalityTags[0].ToString()
                : "none";

            var combat = explorationEvent.Combat;
            return string.Join("|",
                explorationEvent.EventType,
                explorationEvent.Salience,
                explorationEvent.ZoneId,
                combat?.Outcome,
                combat?.MonsterDisplayName ?? explorationEvent.DiscoveryItemId ?? "none",
                personality);
        }
    }
}
