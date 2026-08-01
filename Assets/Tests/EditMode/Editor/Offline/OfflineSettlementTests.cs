using System;
using Backend.Meta.Currency;
using Backend.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace Backend.GameSystems.Offline.Tests
{
    public class OfflineSettlementTests
    {
        private static readonly DateTimeOffset BaseTime =
            new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

        private TransactionLedger _ledger;
        private Wallet _wallet;
        private BalanceTable _table;

        [SetUp]
        public void SetUp()
        {
            _ledger = new TransactionLedger(TransactionLedger.DEFAULT_MAX_ENTRIES, () => BaseTime);
            _wallet = new Wallet(_ledger);
            _table = ScriptableObject.CreateInstance<BalanceTable>();
            _table.ApplySpecDefaults();
            BalanceTableProvider.SetForTests(_table);
            OfflineRuntimeProvider.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            BalanceTableProvider.ResetCache();
            OfflineRuntimeProvider.Reset();
        }

        [Test]
        public void OfflineCapCalculator_BaseCap_IsFourHours()
        {
            var cap = OfflineCapCalculator.GetCap(innFacilityLevel: 0, hasActiveMonthlyContract: false);
            Assert.AreEqual(TimeSpan.FromHours(4), cap);
        }

        [Test]
        public void OfflineCapCalculator_InnAndMonthlyContract_ExpandToTwelveHours()
        {
            var cap = OfflineCapCalculator.GetCap(innFacilityLevel: 4, hasActiveMonthlyContract: true);
            Assert.AreEqual(TimeSpan.FromHours(12), cap);
        }

        [Test]
        public void OfflineCapCalculator_InnBonus_IsCappedAtFourHours()
        {
            var cap = OfflineCapCalculator.GetCap(innFacilityLevel: 99, hasActiveMonthlyContract: false);
            Assert.AreEqual(TimeSpan.FromHours(8), cap);
        }

        [Test]
        public void SettleOnReturn_ClampsElapsed_ToCap()
        {
            var service = CreateService(
                localNow: BaseTime,
                lastSettlement: BaseTime.AddHours(-20));

            service.InnFacilityLevel = 0;
            var result = service.SettleOnReturn();

            Assert.AreEqual(TimeSpan.FromHours(20), result.RawElapsed);
            Assert.AreEqual(TimeSpan.FromHours(4), result.SettledDuration);
            Assert.AreEqual(TimeSpan.FromHours(4), result.Cap);
        }

        [Test]
        public void SettleOnReturn_AppliesSeventyPercentEfficiency()
        {
            var elapsed = TimeSpan.FromHours(2);
            var service = CreateService(
                localNow: BaseTime,
                lastSettlement: BaseTime - elapsed);
            service.SetCurrentFloorForTests(1);

            var result = service.SettleOnReturn();
            var fullEfficiencyGold = OfflineRewardCalculator.CalculateGold(
                _table,
                currentFloor: 1,
                settledDuration: elapsed,
                efficiency: 1d);

            Assert.Greater(result.GoldReward, 0L);
            Assert.Less(result.GoldReward, fullEfficiencyGold);
            Assert.AreEqual(
                OfflineRewardCalculator.CalculateGold(_table, 1, elapsed, OfflinePolicy.DefaultEfficiency),
                result.GoldReward);
        }

        [Test]
        public void SettleOnReturn_TimeManipulation_ReturnsZeroReward()
        {
            var lastSettlement = BaseTime;
            var manipulatedNow = BaseTime.AddHours(-3);

            var service = CreateService(
                localNow: manipulatedNow,
                lastSettlement: lastSettlement,
                serverTimeProvider: null);

            var result = service.SettleOnReturn();

            Assert.AreEqual(TimeSpan.Zero, result.RawElapsed);
            Assert.AreEqual(TimeSpan.Zero, result.SettledDuration);
            Assert.AreEqual(0L, result.GoldReward);
            Assert.IsTrue(result.TimeManipulationBlocked);
            Assert.IsFalse(result.ShouldShowSummary);
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.Gold));
        }

        [Test]
        public void SettleOnReturn_UsesServerTime_WhenAvailable()
        {
            var serverNow = BaseTime.AddHours(3);
            var localNow = BaseTime.AddHours(-5);
            var lastSettlement = BaseTime;

            var service = CreateService(
                localNow: localNow,
                lastSettlement: lastSettlement,
                serverTimeProvider: new FixedServerTimeProvider(serverNow));

            var result = service.SettleOnReturn();

            Assert.AreEqual(TimeSpan.FromHours(3), result.RawElapsed);
            Assert.IsFalse(result.UsedLocalFallback);
            Assert.IsFalse(result.TimeManipulationBlocked);
        }

        [Test]
        public void SettleOnReturn_CreditsWallet_AndBuildsHighlights()
        {
            var service = CreateService(
                localNow: BaseTime,
                lastSettlement: BaseTime.AddHours(-2),
                narrationBuilder: request => $"line:{request.EventType}");

            service.SetCurrentFloorForTests(3);
            var result = service.SettleOnReturn();

            Assert.IsTrue(result.ShouldShowSummary);
            Assert.IsTrue(result.AppliedToWallet);
            Assert.Greater(result.GoldReward, 0L);
            Assert.AreEqual(result.GoldReward, _wallet.GetBalance(CurrencyType.Gold));
            Assert.GreaterOrEqual(result.Highlights.Count, OfflinePolicy.MinHighlightCount);
            Assert.LessOrEqual(result.Highlights.Count, OfflinePolicy.MaxHighlightCount);
        }

        [Test]
        public void SettleOnReturn_ShortAbsence_DoesNotShowSummary()
        {
            var service = CreateService(
                localNow: BaseTime,
                lastSettlement: BaseTime.AddSeconds(30));

            var result = service.SettleOnReturn();

            Assert.IsFalse(result.ShouldShowSummary);
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.Gold));
        }

        private OfflineSettlementService CreateService(
            DateTimeOffset localNow,
            DateTimeOffset lastSettlement,
            IServerTimeProvider serverTimeProvider = null,
            Func<Backend.Chronicle.NarrationRequest, string> narrationBuilder = null)
        {
            var service = new OfflineSettlementService(
                _wallet,
                _table,
                serverTimeProvider,
                () => localNow,
                narrationBuilder);

            service.SetLastSettlementUtcForTests(lastSettlement);
            return service;
        }

        private sealed class FixedServerTimeProvider : IServerTimeProvider
        {
            private readonly DateTimeOffset _time;

            public FixedServerTimeProvider(DateTimeOffset time)
            {
                _time = time;
            }

            public bool TryGetServerTimeUtc(out DateTimeOffset serverTimeUtc)
            {
                serverTimeUtc = _time;
                return true;
            }
        }
    }
}
