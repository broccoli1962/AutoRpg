using System;
using System.Collections.Generic;

namespace Backend.Meta.Currency
{
    /// <summary>
    /// 재화 변동 원장. 최근 N건을 유지해 세이브·분석·CS 대응에 사용한다.
    /// </summary>
    public sealed class TransactionLedger
    {
        public const int DEFAULT_MAX_ENTRIES = 100;

        private readonly int _maxEntries;
        private readonly Func<DateTimeOffset> _utcNow;
        private readonly List<LedgerEntry> _entries = new();

        public TransactionLedger(
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
        public IReadOnlyList<LedgerEntry> Entries => _entries;

        /// <summary>
        /// 최근 기록 상한을 반환한다.
        /// </summary>
        public int MaxEntries => _maxEntries;

        /// <summary>
        /// 재화 변동 1건을 기록한다.
        /// </summary>
        public LedgerEntry Record(
            string reasonCode,
            CurrencyType currencyType,
            long delta,
            long balanceAfter)
        {
            var entry = new LedgerEntry(
                reasonCode ?? string.Empty,
                currencyType,
                delta,
                balanceAfter,
                _utcNow().UtcTicks);

            _entries.Add(entry);

            while (_entries.Count > _maxEntries)
                _entries.RemoveAt(0);

            return entry;
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public TransactionLedgerSaveData ToSaveData()
        {
            return new TransactionLedgerSaveData
            {
                MaxEntries = _maxEntries,
                Entries = _entries.ToArray(),
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 원장을 복원한다.
        /// </summary>
        public static TransactionLedger FromSaveData(
            TransactionLedgerSaveData saveData,
            Func<DateTimeOffset> utcNow = null)
        {
            if (saveData == null)
                return new TransactionLedger(DEFAULT_MAX_ENTRIES, utcNow);

            var maxEntries = saveData.MaxEntries > 0
                ? saveData.MaxEntries
                : DEFAULT_MAX_ENTRIES;

            var ledger = new TransactionLedger(maxEntries, utcNow);

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
