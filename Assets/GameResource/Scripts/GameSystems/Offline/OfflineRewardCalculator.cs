using System;
using Backend.Meta.Currency;
using Backend.Simulation;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 오프라인 구간 보상(골드 등)을 온라인 대비 효율로 추정한다.
    /// </summary>
    public static class OfflineRewardCalculator
    {
        /// <summary>
        /// 정산 구간·층·효율에 따른 골드 보상을 계산한다.
        /// </summary>
        public static long CalculateGold(
            BalanceTable table,
            int currentFloor,
            TimeSpan settledDuration,
            double efficiency = OfflinePolicy.DefaultEfficiency)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (settledDuration <= TimeSpan.Zero || currentFloor < 1)
                return 0L;

            if (efficiency <= 0d)
                return 0L;

            var goldDrop = BalanceFormulas.GetGoldDrop(table, currentFloor);
            var goldPerSecondOnline = goldDrop * OfflinePolicy.OnlineKillsPerSecond;
            var total = goldPerSecondOnline * settledDuration.TotalSeconds * efficiency;
            if (total <= 0d || double.IsNaN(total) || double.IsInfinity(total))
                return 0L;

            return (long)Math.Floor(total);
        }

        /// <summary>
        /// 경과 시간을 상한으로 절삭한 뒤 보상 목록을 생성한다.
        /// </summary>
        public static OfflineRewardBundle BuildRewards(
            BalanceTable table,
            int currentFloor,
            TimeSpan elapsed,
            TimeSpan cap,
            double efficiency = OfflinePolicy.DefaultEfficiency)
        {
            var settledDuration = elapsed > cap ? cap : elapsed;
            var gold = CalculateGold(table, currentFloor, settledDuration, efficiency);

            return new OfflineRewardBundle(settledDuration, gold);
        }
    }

    /// <summary>
    /// 오프라인 정산 보상 묶음.
    /// </summary>
    public readonly struct OfflineRewardBundle
    {
        public OfflineRewardBundle(TimeSpan settledDuration, long gold)
        {
            SettledDuration = settledDuration;
            Gold = gold;
        }

        public TimeSpan SettledDuration { get; }
        public long Gold { get; }

        /// <summary>
        /// 지급할 보상이 있는지 여부.
        /// </summary>
        public bool HasRewards => Gold > 0L;

        /// <summary>
        /// Wallet 에 보상을 적용한다.
        /// </summary>
        public void ApplyTo(Wallet wallet)
        {
            if (wallet == null || !HasRewards)
                return;

            wallet.TryCredit(CurrencyType.Gold, Gold, CurrencyReasonCodes.OfflineReward);
        }
    }
}
