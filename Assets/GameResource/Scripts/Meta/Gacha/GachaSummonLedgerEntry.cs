using System;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 소환 원장 1건. 시드·배너·결과를 세이브·CS 대조용으로 기록한다.
    /// </summary>
    [Serializable]
    public struct GachaSummonLedgerEntry
    {
        public long Seed;
        public string BannerId;
        public int PullCount;
        public long TimestampUtcTicks;
        public GachaPullRecord[] Pulls;

        public GachaSummonLedgerEntry(
            long seed,
            string bannerId,
            int pullCount,
            long timestampUtcTicks,
            GachaPullRecord[] pulls)
        {
            Seed = seed;
            BannerId = bannerId ?? string.Empty;
            PullCount = pullCount;
            TimestampUtcTicks = timestampUtcTicks;
            Pulls = pulls ?? Array.Empty<GachaPullRecord>();
        }

        public DateTimeOffset TimestampUtc =>
            new DateTimeOffset(TimestampUtcTicks, TimeSpan.Zero);
    }
}
