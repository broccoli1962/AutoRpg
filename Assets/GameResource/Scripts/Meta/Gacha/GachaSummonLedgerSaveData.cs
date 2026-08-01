using System;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 소환 원장 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class GachaSummonLedgerSaveData
    {
        public int MaxEntries = GachaSummonLedger.DEFAULT_MAX_ENTRIES;
        public GachaSummonLedgerEntry[] Entries = Array.Empty<GachaSummonLedgerEntry>();
    }
}
