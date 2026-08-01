using System;

namespace Backend.Meta.Currency
{
    /// <summary>
    /// TransactionLedger 세이브 데이터.
    /// </summary>
    [Serializable]
    public sealed class TransactionLedgerSaveData
    {
        public int MaxEntries = TransactionLedger.DEFAULT_MAX_ENTRIES;
        public LedgerEntry[] Entries = Array.Empty<LedgerEntry>();
    }
}
