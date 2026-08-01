using System;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 소환 시스템 세이브 스냅샷. 천장 카운터는 배너와 무관하게 공유된다.
    /// </summary>
    [Serializable]
    public sealed class GachaSaveData
    {
        public GachaPitySaveData Pity = new();
        public GachaSummonLedgerSaveData Ledger = new();
        public long NextSeed = 1L;
    }
}
