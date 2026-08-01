using System;
using System.Collections.Generic;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 소환 결과 원장. 시드와 추첨 결과를 최근 N건 유지한다.
    /// </summary>
    public sealed class GachaSummonLedger
    {
        public const int DEFAULT_MAX_ENTRIES = 100;

        private readonly int _maxEntries;
        private readonly Func<DateTimeOffset> _utcNow;
        private readonly List<GachaSummonLedgerEntry> _entries = new();

        public GachaSummonLedger(
            int maxEntries = DEFAULT_MAX_ENTRIES,
            Func<DateTimeOffset> utcNow = null)
        {
            if (maxEntries < 1)
                throw new ArgumentOutOfRangeException(nameof(maxEntries));

            _maxEntries = maxEntries;
            _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 기록된 전체 항목(최대 N건)을 반환한다.
        /// </summary>
        public IReadOnlyList<GachaSummonLedgerEntry> Entries => _entries;

        /// <summary>
        /// 소환 1세션을 기록한다.
        /// </summary>
        public GachaSummonLedgerEntry Record(
            long seed,
            string bannerId,
            GachaPullRecord[] pulls)
        {
            var entry = new GachaSummonLedgerEntry(
                seed,
                bannerId,
                pulls?.Length ?? 0,
                _utcNow().UtcTicks,
                pulls);

            _entries.Add(entry);

            while (_entries.Count > _maxEntries)
                _entries.RemoveAt(0);

            return entry;
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public GachaSummonLedgerSaveData ToSaveData()
        {
            return new GachaSummonLedgerSaveData
            {
                MaxEntries = _maxEntries,
                Entries = _entries.ToArray(),
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 원장을 복원한다.
        /// </summary>
        public static GachaSummonLedger FromSaveData(
            GachaSummonLedgerSaveData saveData,
            Func<DateTimeOffset> utcNow = null)
        {
            if (saveData == null)
                return new GachaSummonLedger(DEFAULT_MAX_ENTRIES, utcNow);

            var maxEntries = saveData.MaxEntries > 0
                ? saveData.MaxEntries
                : DEFAULT_MAX_ENTRIES;

            var ledger = new GachaSummonLedger(maxEntries, utcNow);

            if (saveData.Entries == null || saveData.Entries.Length == 0)
                return ledger;

            foreach (var entry in saveData.Entries)
                ledger._entries.Add(entry);

            while (ledger._entries.Count > ledger._maxEntries)
                ledger._entries.RemoveAt(0);

            return ledger;
        }
    }
}
