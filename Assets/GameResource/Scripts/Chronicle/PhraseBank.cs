using System;
using System.Collections.Generic;

namespace Backend.Chronicle
{
    /// <summary>
    /// 이벤트 타입별 슬롯 풀을 보관하는 문장 뱅크.
    /// </summary>
    public sealed class PhraseBank
    {
        private readonly Dictionary<string, Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>> _pools;

        /// <summary>
        /// 이벤트 타입 → 슬롯 → 항목 목록 구조로 문장 뱅크를 구성한다.
        /// </summary>
        public PhraseBank(
            IReadOnlyDictionary<string, IReadOnlyDictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>> pools)
        {
            _pools = new Dictionary<string, Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>>(
                StringComparer.Ordinal);

            if (pools == null)
                return;

            foreach (var eventPair in pools)
            {
                if (string.IsNullOrEmpty(eventPair.Key) || eventPair.Value == null)
                    continue;

                var slotMap = new Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>();
                foreach (var slotPair in eventPair.Value)
                {
                    slotMap[slotPair.Key] = slotPair.Value ?? Array.Empty<PhraseEntry>();
                }

                _pools[eventPair.Key] = slotMap;
            }
        }

        /// <summary>
        /// 지정 이벤트·슬롯의 항목 목록을 반환한다. 없으면 빈 목록.
        /// </summary>
        public IReadOnlyList<PhraseEntry> GetEntries(string eventType, PhraseSlot slot)
        {
            if (string.IsNullOrEmpty(eventType))
                return Array.Empty<PhraseEntry>();

            if (!_pools.TryGetValue(eventType, out var slotMap))
                return Array.Empty<PhraseEntry>();

            return slotMap.TryGetValue(slot, out var entries)
                ? entries
                : Array.Empty<PhraseEntry>();
        }
    }
}
