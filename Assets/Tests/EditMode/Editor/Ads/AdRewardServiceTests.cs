using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Meta.Retention;
using Backend.Meta.Shop;
using Backend.Simulation;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.Ads.Tests
{
    public class AdRewardServiceTests
    {
        private static readonly DateTimeOffset DayOneUtc =
            new DateTimeOffset(2026, 7, 31, 19, 30, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset DayTwoUtc =
            new DateTimeOffset(2026, 8, 1, 19, 30, 0, TimeSpan.Zero);

        private TransactionLedger _ledger;
        private Wallet _wallet;
        private AdConfigTable _config;
        private SimulatedAdService _adService;
        private DefaultTutorialGate _tutorialGate;
        private ShopService _shopService;
        private DateTimeOffset _nowUtc;
        private AdRewardService _service;

        [SetUp]
        public void SetUp()
        {
            _nowUtc = DayOneUtc;
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _config = ScriptableObject.CreateInstance<AdConfigTable>();
            _config.ApplySpecDefaults();
            _adService = new SimulatedAdService();
            _tutorialGate = new DefaultTutorialGate();

            _shopService = new ShopService(
                _wallet,
                new ExplorerCatalog(),
                ScriptableObject.CreateInstance<BalanceTable>(),
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);

            var grantor = new AdRewardGrantor(_wallet, ScriptableObject.CreateInstance<BalanceTable>(), () => 10);
            _service = new AdRewardService(
                _adService,
                _config,
                grantor,
                _tutorialGate,
                () => _shopService.ShouldShowInterstitialAds,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);

            _adService.InitializeAsync(_config).GetAwaiter().GetResult();
            _service.RefreshDailyPeriod();
        }

        [TearDown]
        public void TearDown()
        {
            OfflineReturnSummaryContext.Clear();

            if (_config != null)
                UnityEngine.Object.DestroyImmediate(_config);
        }

        [Test]
        public void AdConfigTable_DefinesFiveRewardPlacementsPlusEventReserve()
        {
            Assert.AreEqual(6, _config.Placements.Count);
            Assert.AreEqual(15, _config.TotalRewardedDailyLimit);
            Assert.AreEqual(6, _config.InterstitialDailyLimit);
            Assert.AreEqual(2, _config.InterstitialSessionLimit);
            Assert.AreEqual(3, _config.FindPlacement(RewardedAdPlacement.OfflineDouble).DailyLimit);
            Assert.AreEqual(5, _config.FindPlacement(RewardedAdPlacement.InstantProgress).DailyLimit);
            Assert.AreEqual(1, _config.FindPlacement(RewardedAdPlacement.FreeSummonTicket).DailyLimit);
            Assert.AreEqual(3, _config.FindPlacement(RewardedAdPlacement.InstantRetry).DailyLimit);
            Assert.AreEqual(2, _config.FindPlacement(RewardedAdPlacement.EnhancementBoost).DailyLimit);
            Assert.AreEqual(1, _config.FindPlacement(RewardedAdPlacement.EventReserve).DailyLimit);
        }

        [Test]
        public void TryShowRewardedAsync_GrantsRewardOnlyOnCompletedCallback()
        {
            _adService.SetRewardedOutcomeForTests(AdShowOutcome.Skipped);

            var skipped = _service.TryShowRewardedAsync(RewardedAdPlacement.FreeSummonTicket)
                .GetAwaiter()
                .GetResult();

            Assert.IsFalse(skipped.Success);
            Assert.AreEqual(AdShowOutcome.Skipped, skipped.Outcome);
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.SummonTicket));
            Assert.AreEqual(0, _service.TotalRewardedToday);

            _adService.SetRewardedOutcomeForTests(AdShowOutcome.Completed);

            var completed = _service.TryShowRewardedAsync(RewardedAdPlacement.FreeSummonTicket)
                .GetAwaiter()
                .GetResult();

            Assert.IsTrue(completed.Success);
            Assert.AreEqual(1L, _wallet.GetBalance(CurrencyType.SummonTicket));
            Assert.AreEqual(1, _service.TotalRewardedToday);
            Assert.AreEqual(CurrencyReasonCodes.AdRewardFreeSummon, _ledger.Entries[0].ReasonCode);
        }

        [Test]
        public void TryShowRewardedAsync_DoesNotBlockGameWhenAdNotLoaded()
        {
            _adService.SetRewardedOutcomeForTests(AdShowOutcome.NotLoaded, isReady: false);

            var result = _service.TryShowRewardedAsync(RewardedAdPlacement.InstantProgress)
                .GetAwaiter()
                .GetResult();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AdShowOutcome.NotLoaded, result.Outcome);
            Assert.AreEqual(0, _service.TotalRewardedToday);
        }

        [Test]
        public void TryShowRewardedAsync_EnforcesPlacementAndTotalDailyLimits()
        {
            for (var i = 0; i < 3; i++)
            {
                var result = _service.TryShowRewardedAsync(RewardedAdPlacement.OfflineDouble)
                    .GetAwaiter()
                    .GetResult();
                Assert.IsTrue(result.Success);
            }

            var blocked = _service.TryShowRewardedAsync(RewardedAdPlacement.OfflineDouble)
                .GetAwaiter()
                .GetResult();

            Assert.IsFalse(blocked.Success);
            Assert.AreEqual(AdShowOutcome.Blocked, blocked.Outcome);
            Assert.AreEqual(0, _service.GetRemainingDailyUses(RewardedAdPlacement.OfflineDouble));
        }

        [Test]
        public void RefreshDailyPeriod_ResetsCountersAtKstFourAmBoundary()
        {
            for (var i = 0; i < 2; i++)
            {
                _service.TryShowRewardedAsync(RewardedAdPlacement.InstantRetry)
                    .GetAwaiter()
                    .GetResult();
            }

            Assert.AreEqual(2, _service.TotalRewardedToday);

            _nowUtc = DayTwoUtc;
            _service.RefreshDailyPeriod();

            Assert.AreEqual(0, _service.TotalRewardedToday);
            Assert.AreEqual(3, _service.GetRemainingDailyUses(RewardedAdPlacement.InstantRetry));
        }

        [Test]
        public void TryShowRewardedAsync_BlockedDuringTutorial()
        {
            _tutorialGate.IsTutorialActive = true;

            Assert.IsFalse(_service.CanShowRewarded(RewardedAdPlacement.FreeSummonTicket));

            var result = _service.TryShowRewardedAsync(RewardedAdPlacement.FreeSummonTicket)
                .GetAwaiter()
                .GetResult();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(AdShowOutcome.Blocked, result.Outcome);
        }

        [Test]
        public void TryShowInterstitialAsync_TracksDailyAndSessionLimitsSeparatelyFromRewarded()
        {
            _service.ResetSessionCounters();

            var first = _service.TryShowInterstitialAsync(InterstitialTrigger.FloorTransition)
                .GetAwaiter()
                .GetResult();
            var second = _service.TryShowInterstitialAsync(InterstitialTrigger.FloorTransition)
                .GetAwaiter()
                .GetResult();
            var third = _service.TryShowInterstitialAsync(InterstitialTrigger.FloorTransition)
                .GetAwaiter()
                .GetResult();

            Assert.AreEqual(AdShowOutcome.Completed, first);
            Assert.AreEqual(AdShowOutcome.Completed, second);
            Assert.AreEqual(AdShowOutcome.Blocked, third);
            Assert.AreEqual(2, _service.InterstitialSessionCount);
            Assert.AreEqual(2, _service.InterstitialDailyCount);
            Assert.AreEqual(0, _service.TotalRewardedToday);
        }

        [Test]
        public void TryShowInterstitialAsync_SkippedWhenAdRemovalOwned()
        {
            _shopService.FulfillValidatedPurchase(
                new ShopProductDefinition
                {
                    ProductId = "ad_removal",
                    Category = ShopProductCategory.AdRemoval,
                    Entitlement = ShopEntitlementType.PermanentAdRemoval,
                },
                "tx_ad_removal_test",
                null);

            Assert.IsFalse(_shopService.ShouldShowInterstitialAds);
            Assert.IsFalse(_service.CanShowInterstitial());

            var outcome = _service.TryShowInterstitialAsync(InterstitialTrigger.SeasonTransition)
                .GetAwaiter()
                .GetResult();

            Assert.AreEqual(AdShowOutcome.Skipped, outcome);
            Assert.AreEqual(0, _service.InterstitialDailyCount);
        }

        [Test]
        public void TryShowRewardedAsync_OfflineDoubleGrantsBonusGoldAtFullEfficiency()
        {
            OfflineReturnSummaryContext.SetPending(new OfflineSettlementResult
            {
                GoldReward = 700L,
                AppliedToWallet = true,
            });

            var result = _service.TryShowRewardedAsync(RewardedAdPlacement.OfflineDouble)
                .GetAwaiter()
                .GetResult();

            Assert.IsTrue(result.Success);
            Assert.Greater(result.GrantedGold, 0L);
            Assert.AreEqual(CurrencyReasonCodes.AdRewardOfflineDouble, _ledger.Entries[0].ReasonCode);
        }

        [Test]
        public void SaveAndLoad_PreservesDailyCountersAndBuffState()
        {
            _service.TryShowRewardedAsync(RewardedAdPlacement.InstantRetry)
                .GetAwaiter()
                .GetResult();
            _service.TryShowRewardedAsync(RewardedAdPlacement.EnhancementBoost)
                .GetAwaiter()
                .GetResult();

            var saveData = _service.ToSaveData();

            var freshGrantor = new AdRewardGrantor(_wallet, ScriptableObject.CreateInstance<BalanceTable>(), () => 1);
            var freshService = new AdRewardService(
                _adService,
                _config,
                freshGrantor,
                _tutorialGate,
                () => true,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);
            freshService.LoadSaveData(saveData);

            Assert.AreEqual(2, freshService.TotalRewardedToday);
            Assert.AreEqual(2, freshService.GetRemainingDailyUses(RewardedAdPlacement.InstantRetry));
            Assert.IsTrue(freshService.BuffState.HasInstantRetryToken);
            Assert.AreEqual(3, freshService.BuffState.EnhancementDiscountRemainingUses);
        }

        private sealed class FixedServerTimeProvider : IServerTimeProvider
        {
            private readonly Func<DateTimeOffset> _nowProvider;

            public FixedServerTimeProvider(Func<DateTimeOffset> nowProvider)
            {
                _nowProvider = nowProvider;
            }

            public bool TryGetServerTimeUtc(out DateTimeOffset serverTimeUtc)
            {
                serverTimeUtc = _nowProvider();
                return true;
            }
        }
    }
}
