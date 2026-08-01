using System;

namespace Backend.Meta.Currency
{
    /// <summary>
    /// 거래 원장 1건. 세이브·CS 분석용으로 직렬화 가능하다.
    /// </summary>
    [Serializable]
    public struct LedgerEntry
    {
        public string ReasonCode;
        public CurrencyType CurrencyType;
        public long Delta;
        public long BalanceAfter;
        public long TimestampUtcTicks;

        public LedgerEntry(
            string reasonCode,
            CurrencyType currencyType,
            long delta,
            long balanceAfter,
            long timestampUtcTicks)
        {
            ReasonCode = reasonCode;
            CurrencyType = currencyType;
            Delta = delta;
            BalanceAfter = balanceAfter;
            TimestampUtcTicks = timestampUtcTicks;
        }

        public DateTimeOffset TimestampUtc =>
            new DateTimeOffset(TimestampUtcTicks, TimeSpan.Zero);
    }
}
