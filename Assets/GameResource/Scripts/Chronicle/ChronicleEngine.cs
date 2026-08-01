using System;
using System.Collections.Generic;
using System.Text;
using Backend.Util.Localization;

namespace Backend.Chronicle
{
    /// <summary>
    /// 작가 집필 문장 뱅크를 조합해 연대기 한 줄을 생성하는 순수 C# 코어.
    /// </summary>
    public sealed class ChronicleEngine
    {
        private const int RECENT_COMBINATION_CAPACITY = 20;
        private const int MAX_AVOID_REPEAT_ATTEMPTS = 32;

        private static readonly PhraseSlot[] AllSlots =
        {
            PhraseSlot.Intro,
            PhraseSlot.Action,
            PhraseSlot.Result,
            PhraseSlot.Afterglow
        };

        private static readonly IReadOnlyDictionary<PhraseSlot, PhraseEntry> FallbackEntries =
            new Dictionary<PhraseSlot, PhraseEntry>
            {
                [PhraseSlot.Intro] = new PhraseEntry("chronicle.fallback.intro", 1, Array.Empty<string>()),
                [PhraseSlot.Action] = new PhraseEntry("chronicle.fallback.action", 1, Array.Empty<string>()),
                [PhraseSlot.Result] = new PhraseEntry("chronicle.fallback.result", 1, Array.Empty<string>()),
                [PhraseSlot.Afterglow] = new PhraseEntry("chronicle.fallback.afterglow", 1, Array.Empty<string>())
            };

        private readonly PhraseBank _bank;
        private readonly RecentCombinationBuffer _recentCombinations;

        /// <summary>
        /// 문장 뱅크를 사용하는 연대기 엔진을 생성한다.
        /// </summary>
        public ChronicleEngine(PhraseBank bank)
        {
            _bank = bank ?? new PhraseBank(null);
            _recentCombinations = new RecentCombinationBuffer(RECENT_COMBINATION_CAPACITY);
        }

        /// <summary>
        /// 이벤트·태그 조건으로 4구를 조합한 문장을 생성한다.
        /// </summary>
        public string Generate(ChronicleGenerationRequest request, IRandomSource random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var contextTags = BuildContextTags(request);
            var slotCandidates = BuildSlotCandidates(request?.EventType, contextTags);
            var picks = PickCombination(slotCandidates, random);
            _recentCombinations.Add(picks.CombinationKey);

            var resolveText = request?.ResolveText;
            var variables = request?.Variables;
            var parts = new string[AllSlots.Length];

            for (var i = 0; i < AllSlots.Length; i++)
            {
                var slot = AllSlots[i];
                var text = ResolveEntryText(picks.Entries[slot], resolveText);
                parts[i] = SubstituteVariables(text, variables);
            }

            var sentence = JoinParts(parts);
            return string.IsNullOrWhiteSpace(sentence)
                ? LocalizationService.Get(PassthroughNarrationSource.DefaultFallbackKey)
                : sentence;
        }

        private static HashSet<string> BuildContextTags(ChronicleGenerationRequest request)
        {
            var tags = new HashSet<string>(StringComparer.Ordinal);

            if (request?.CharacterPersonalityTags != null)
            {
                foreach (var tag in request.CharacterPersonalityTags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                        tags.Add(tag.Trim());
                }
            }

            if (!string.IsNullOrWhiteSpace(request?.ZoneToneTag))
                tags.Add(request.ZoneToneTag.Trim());

            return tags;
        }

        private Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>> BuildSlotCandidates(
            string eventType,
            HashSet<string> contextTags)
        {
            var result = new Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>();

            foreach (var slot in AllSlots)
            {
                var pool = _bank.GetEntries(eventType, slot);
                var filtered = FilterEntries(pool, contextTags);

                if (filtered.Count == 0 && pool.Count > 0)
                    filtered = pool;

                if (filtered.Count == 0)
                    filtered = new[] { FallbackEntries[slot] };

                result[slot] = filtered;
            }

            return result;
        }

        private static IReadOnlyList<PhraseEntry> FilterEntries(
            IReadOnlyList<PhraseEntry> entries,
            HashSet<string> contextTags)
        {
            if (entries == null || entries.Count == 0)
                return Array.Empty<PhraseEntry>();

            var matched = new List<PhraseEntry>();
            foreach (var entry in entries)
            {
                if (entry == null)
                    continue;

                if (MatchesConditions(entry, contextTags))
                    matched.Add(entry);
            }

            return matched;
        }

        private static bool MatchesConditions(PhraseEntry entry, HashSet<string> contextTags)
        {
            if (entry.ConditionTags == null || entry.ConditionTags.Count == 0)
                return true;

            foreach (var tag in entry.ConditionTags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                if (!contextTags.Contains(tag.Trim()))
                    return false;
            }

            return true;
        }

        private CombinationPick PickCombination(
            Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>> slotCandidates,
            IRandomSource random)
        {
            for (var attempt = 0; attempt < MAX_AVOID_REPEAT_ATTEMPTS; attempt++)
            {
                var pick = PickWeightedCombination(slotCandidates, random);
                if (!_recentCombinations.Contains(pick.CombinationKey))
                    return pick;
            }

            return PickLowestWeightCombination(slotCandidates);
        }

        private static CombinationPick PickWeightedCombination(
            Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>> slotCandidates,
            IRandomSource random)
        {
            var entries = new Dictionary<PhraseSlot, PhraseEntry>();
            var keyParts = new string[AllSlots.Length];

            for (var i = 0; i < AllSlots.Length; i++)
            {
                var slot = AllSlots[i];
                var entry = PickWeightedEntry(slotCandidates[slot], random);
                entries[slot] = entry;
                keyParts[i] = entry.LocalizationKey ?? string.Empty;
            }

            return new CombinationPick(entries, string.Join("|", keyParts));
        }

        private static CombinationPick PickLowestWeightCombination(
            Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>> slotCandidates)
        {
            var entries = new Dictionary<PhraseSlot, PhraseEntry>();
            var keyParts = new string[AllSlots.Length];

            for (var i = 0; i < AllSlots.Length; i++)
            {
                var slot = AllSlots[i];
                var entry = PickLowestWeightEntry(slotCandidates[slot]);
                entries[slot] = entry;
                keyParts[i] = entry.LocalizationKey ?? string.Empty;
            }

            return new CombinationPick(entries, string.Join("|", keyParts));
        }

        private static PhraseEntry PickWeightedEntry(IReadOnlyList<PhraseEntry> candidates, IRandomSource random)
        {
            if (candidates == null || candidates.Count == 0)
                return FallbackEntries[PhraseSlot.Intro];

            if (candidates.Count == 1)
                return candidates[0];

            var totalWeight = 0;
            foreach (var candidate in candidates)
                totalWeight += Math.Max(1, candidate.Weight);

            var roll = random.NextInt(0, totalWeight);
            var cumulative = 0;

            foreach (var candidate in candidates)
            {
                cumulative += Math.Max(1, candidate.Weight);
                if (roll < cumulative)
                    return candidate;
            }

            return candidates[candidates.Count - 1];
        }

        private static PhraseEntry PickLowestWeightEntry(IReadOnlyList<PhraseEntry> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return FallbackEntries[PhraseSlot.Intro];

            PhraseEntry best = candidates[0];
            var bestWeight = Math.Max(1, best.Weight);
            var bestKey = best.LocalizationKey ?? string.Empty;

            for (var i = 1; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var weight = Math.Max(1, candidate.Weight);
                var key = candidate.LocalizationKey ?? string.Empty;

                if (weight < bestWeight || (weight == bestWeight && string.CompareOrdinal(key, bestKey) < 0))
                {
                    best = candidate;
                    bestWeight = weight;
                    bestKey = key;
                }
            }

            return best;
        }

        private static string ResolveEntryText(PhraseEntry entry, Func<string, string> resolveText)
        {
            var key = entry?.LocalizationKey ?? string.Empty;
            if (string.IsNullOrEmpty(key))
                return LocalizationService.Get(PassthroughNarrationSource.DefaultFallbackKey);

            var resolved = resolveText != null ? resolveText(key) : key;
            if (string.IsNullOrWhiteSpace(resolved))
                resolved = key;

            return resolved.Trim();
        }

        /// <summary>
        /// {variable} 형태의 슬롯을 실제 값으로 치환한다.
        /// </summary>
        public static string SubstituteVariables(string text, IReadOnlyDictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(text) || variables == null || variables.Count == 0)
                return text ?? string.Empty;

            var builder = new StringBuilder(text.Length + 16);
            var index = 0;

            while (index < text.Length)
            {
                var openBrace = text.IndexOf('{', index);
                if (openBrace < 0)
                {
                    builder.Append(text, index, text.Length - index);
                    break;
                }

                builder.Append(text, index, openBrace - index);
                var closeBrace = text.IndexOf('}', openBrace + 1);
                if (closeBrace < 0)
                {
                    builder.Append(text, openBrace, text.Length - openBrace);
                    break;
                }

                var key = text.Substring(openBrace + 1, closeBrace - openBrace - 1);
                if (variables.TryGetValue(key, out var value) && value != null)
                    builder.Append(value);
                else
                    builder.Append('{').Append(key).Append('}');

                index = closeBrace + 1;
            }

            return builder.ToString();
        }

        private static string JoinParts(IReadOnlyList<string> parts)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (string.IsNullOrEmpty(part))
                    continue;

                if (builder.Length > 0 && !part.StartsWith(".", StringComparison.Ordinal)
                    && !part.StartsWith(",", StringComparison.Ordinal)
                    && !part.StartsWith("!", StringComparison.Ordinal)
                    && !part.StartsWith("?", StringComparison.Ordinal))
                {
                    builder.Append(' ');
                }

                builder.Append(part);
            }

            return builder.ToString().Trim();
        }

        private sealed class CombinationPick
        {
            public CombinationPick(Dictionary<PhraseSlot, PhraseEntry> entries, string combinationKey)
            {
                Entries = entries;
                CombinationKey = combinationKey;
            }

            public Dictionary<PhraseSlot, PhraseEntry> Entries { get; }
            public string CombinationKey { get; }
        }

        private sealed class RecentCombinationBuffer
        {
            private readonly string[] _items;
            private readonly bool[] _occupied;
            private int _head;
            private int _count;

            public RecentCombinationBuffer(int capacity)
            {
                _items = new string[capacity];
                _occupied = new bool[capacity];
            }

            public bool Contains(string combinationKey)
            {
                if (string.IsNullOrEmpty(combinationKey))
                    return false;

                for (var i = 0; i < _count; i++)
                {
                    var index = (_head - 1 - i + _items.Length) % _items.Length;
                    if (_occupied[index] && _items[index] == combinationKey)
                        return true;
                }

                return false;
            }

            public void Add(string combinationKey)
            {
                if (string.IsNullOrEmpty(combinationKey))
                    return;

                _items[_head] = combinationKey;
                _occupied[_head] = true;
                _head = (_head + 1) % _items.Length;

                if (_count < _items.Length)
                    _count++;
            }
        }
    }
}
