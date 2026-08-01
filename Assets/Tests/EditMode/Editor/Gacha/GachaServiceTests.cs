using System.Linq;
using Backend.Chronicle;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.Gacha.Tests
{
    public class GachaServiceTests
    {
        private static readonly System.DateTimeOffset FixedNow =
            new System.DateTimeOffset(2026, 8, 1, 12, 0, 0, System.TimeSpan.Zero);

        private BalanceTable _balanceTable;
        private GachaRateTable _rateTable;
        private TestCharacterPool _pool;
        private Wallet _wallet;
        private ExplorerCatalog _catalog;
        private GachaSummonLedger _ledger;
        private GachaPityState _pity;
        private GachaService _service;

        [SetUp]
        public void SetUp()
        {
            _balanceTable = ScriptableObject.CreateInstance<BalanceTable>();
            _balanceTable.ApplySpecDefaults();

            _rateTable = ScriptableObject.CreateInstance<GachaRateTable>();
            _rateTable.ApplySpecDefaults();

            _pool = new TestCharacterPool();
            _ledger = new GachaSummonLedger(GachaSummonLedger.DEFAULT_MAX_ENTRIES, () => FixedNow);
            _pity = new GachaPityState();

            var currencyLedger = new TransactionLedger(
                TransactionLedger.DEFAULT_MAX_ENTRIES,
                () => FixedNow);
            _wallet = new Wallet(currencyLedger);
            _wallet.TryCredit(CurrencyType.AbyssStone, 1_000_000L, CurrencyReasonCodes.IapGrant);

            _catalog = new ExplorerCatalog();
            _service = new GachaService(
                _wallet,
                _catalog,
                _ledger,
                _pity,
                _balanceTable,
                nextSeed: 42L);
        }

        [TearDown]
        public void TearDown()
        {
            if (_balanceTable != null)
                UnityEngine.Object.DestroyImmediate(_balanceTable);

            if (_rateTable != null)
                UnityEngine.Object.DestroyImmediate(_rateTable);
        }

        [Test]
        public void GachaRateTable_RatesSumToExactlyOneHundredPercent()
        {
            Assert.DoesNotThrow(() => _rateTable.ValidateRates());
            Assert.AreEqual(10_000, _rateTable.RateR + _rateTable.RateSr + _rateTable.RateSsr + _rateTable.RateUr);
        }

        [Test]
        public void GachaRateTable_RejectsInvalidRateSum()
        {
            var invalid = ScriptableObject.CreateInstance<GachaRateTable>();
            invalid.ApplySpecDefaults();

            try
            {
                typeof(GachaRateTable)
                    .GetField("_rateR", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(invalid, 7_001);

                Assert.Throws<System.InvalidOperationException>(() => invalid.ValidateRates());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(invalid);
            }
        }

        [Test]
        public void TryTenSummon_AlwaysIncludesAtLeastOneSrPlus()
        {
            _service = new GachaService(
                _wallet,
                _catalog,
                _ledger,
                _pity,
                _balanceTable,
                nextSeed: 1L,
                seedProvider: () => 1L);

            for (var attempt = 0; attempt < 20; attempt++)
            {
                var result = _service.TryTenSummon(_rateTable, _pool);
                Assert.IsTrue(result.Success, $"Attempt {attempt} failed.");

                Assert.IsTrue(
                    result.Pulls.Any(pull => pull.Grade >= ExplorerGrade.SR),
                    $"Attempt {attempt} did not guarantee SR+.");
            }
        }

        [Test]
        public void SsrPity_TriggersExactlyOnHundredthPullWithoutSsrPlus()
        {
            var alwaysRandom = new AlwaysLowRandomSource();
            var service = CreateServiceWithRandom(alwaysRandom);

            ExplorerGrade? pityGrade = null;

            for (var i = 1; i <= 99; i++)
            {
                var result = service.TrySingleSummon(_rateTable, _pool);
                Assert.IsTrue(result.Success);
                Assert.IsTrue(result.Pulls[0].Grade < ExplorerGrade.SSR, $"Pull {i} should stay below SSR.");
            }

            Assert.AreEqual(99, service.Pity.GetSsrCounter());

            var pityResult = service.TrySingleSummon(_rateTable, _pool);
            Assert.IsTrue(pityResult.Success);
            pityGrade = pityResult.Pulls[0].Grade;

            Assert.IsTrue(pityGrade >= ExplorerGrade.SSR);
            Assert.IsTrue(pityResult.Pulls[0].TriggeredSsrPity);
            Assert.AreEqual(0, service.Pity.GetSsrCounter());
        }

        [Test]
        public void UrPity_TriggersExactlyOnTwoHundredthPullWithoutUr()
        {
            var alwaysRandom = new AlwaysLowRandomSource();
            var service = CreateServiceWithRandom(alwaysRandom);

            for (var i = 1; i <= 199; i++)
            {
                var result = service.TrySingleSummon(_rateTable, _pool);
                Assert.IsTrue(result.Success);
                Assert.AreNotEqual(ExplorerGrade.UR, result.Pulls[0].Grade);
            }

            Assert.AreEqual(199, service.Pity.GetUrCounter());

            var pityResult = service.TrySingleSummon(_rateTable, _pool);
            Assert.IsTrue(pityResult.Success);
            Assert.AreEqual(ExplorerGrade.UR, pityResult.Pulls[0].Grade);
            Assert.IsTrue(pityResult.Pulls[0].TriggeredUrPity);
            Assert.AreEqual(0, service.Pity.GetUrCounter());
        }

        [Test]
        public void SsrAcquisition_ResetsSsrCounter_ButNotUrCounter()
        {
            var sequence = new QueueRandomSource(0.0, 0.96, 0.0);
            var service = CreateServiceWithRandom(sequence);

            service.TrySingleSummon(_rateTable, _pool);
            Assert.AreEqual(1, service.Pity.GetSsrCounter());
            Assert.AreEqual(1, service.Pity.GetUrCounter());

            var ssrResult = service.TrySingleSummon(_rateTable, _pool);
            Assert.IsTrue(ssrResult.Success);
            Assert.AreEqual(ExplorerGrade.SSR, ssrResult.Pulls[0].Grade);
            Assert.AreEqual(0, service.Pity.GetSsrCounter());
            Assert.AreEqual(2, service.Pity.GetUrCounter());
        }

        [Test]
        public void PityCounters_CarryOverAcrossBanners()
        {
            var alwaysRandom = new AlwaysLowRandomSource();
            var service = CreateServiceWithRandom(alwaysRandom);
            var bannerA = new TestCharacterPool("banner_a");
            var bannerB = new TestCharacterPool("banner_b");

            for (var i = 0; i < 30; i++)
                service.TrySingleSummon(_rateTable, bannerA);

            Assert.AreEqual(30, service.Pity.GetSsrCounter());
            Assert.AreEqual(30, service.Pity.GetUrCounter());

            service.TrySingleSummon(_rateTable, bannerB);

            Assert.AreEqual(31, service.Pity.GetSsrCounter());
            Assert.AreEqual(31, service.Pity.GetUrCounter());
        }

        [Test]
        public void SaveAndLoad_PreservesPityCountersAndLedger()
        {
            var alwaysRandom = new AlwaysLowRandomSource();
            var service = CreateServiceWithRandom(alwaysRandom);

            service.TrySingleSummon(_rateTable, _pool);
            service.TryTenSummon(_rateTable, _pool);

            var saveData = service.ToSaveData();
            var restored = GachaService.FromSaveData(
                saveData,
                _wallet,
                _catalog,
                _balanceTable,
                () => FixedNow);

            Assert.AreEqual(service.Pity.GetSsrCounter(), restored.Pity.GetSsrCounter());
            Assert.AreEqual(service.Pity.GetUrCounter(), restored.Pity.GetUrCounter());
            Assert.AreEqual(service.Ledger.Entries.Count, restored.Ledger.Entries.Count);
            Assert.AreEqual(service.Ledger.Entries[0].Seed, restored.Ledger.Entries[0].Seed);
        }

        [Test]
        public void Summon_RecordsSeedAndResultsInLedger()
        {
            var result = _service.TrySingleSummon(_rateTable, _pool);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(42L, result.Seed);
            Assert.AreEqual(1, _ledger.Entries.Count);

            var entry = _ledger.Entries[0];
            Assert.AreEqual(42L, entry.Seed);
            Assert.AreEqual(_pool.BannerId, entry.BannerId);
            Assert.AreEqual(1, entry.PullCount);
            Assert.AreEqual(1, entry.Pulls.Length);
            Assert.AreEqual(result.Pulls[0].Grade, entry.Pulls[0].Grade);
            Assert.AreEqual(result.Pulls[0].CharacterId, entry.Pulls[0].CharacterId);
        }

        [Test]
        public void TrySingleSummon_DebitsThreeHundredAbyssStone()
        {
            var before = _wallet.GetBalance(CurrencyType.AbyssStone);
            var result = _service.TrySingleSummon(_rateTable, _pool);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(before - _rateTable.SinglePullCost, _wallet.GetBalance(CurrencyType.AbyssStone));
        }

        [Test]
        public void TryTenSummon_DebitsTwentySevenHundredAbyssStone()
        {
            var before = _wallet.GetBalance(CurrencyType.AbyssStone);
            var result = _service.TryTenSummon(_rateTable, _pool);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(before - _rateTable.TenPullCost, _wallet.GetBalance(CurrencyType.AbyssStone));
        }

        private GachaService CreateServiceWithRandom(IRandomSource randomSource)
        {
            return new GachaService(
                _wallet,
                _catalog,
                _ledger,
                _pity,
                _balanceTable,
                nextSeed: 1L,
                randomFactory: _ => randomSource);
        }

        private sealed class TestCharacterPool : IGachaCharacterPool
        {
            public TestCharacterPool(string bannerId = "test_banner")
            {
                BannerId = bannerId;
            }

            public string BannerId { get; }

            public string PickCharacter(ExplorerGrade grade, IRandomSource random)
            {
                return grade switch
                {
                    ExplorerGrade.R => "explorer_r_01",
                    ExplorerGrade.SR => "explorer_sr_01",
                    ExplorerGrade.SSR => "explorer_ssr_01",
                    ExplorerGrade.UR => "explorer_ur_01",
                    _ => "explorer_r_01",
                };
            }
        }

        private sealed class AlwaysLowRandomSource : IRandomSource
        {
            public int NextInt(int minInclusive, int maxExclusive) => minInclusive;

            public double NextDouble() => 0.0;
        }

        private sealed class QueueRandomSource : IRandomSource
        {
            private readonly System.Collections.Generic.Queue<double> _values;

            public QueueRandomSource(params double[] values)
            {
                _values = new System.Collections.Generic.Queue<double>(values);
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                var value = NextDouble();
                var range = maxExclusive - minInclusive;
                return minInclusive + (int)(value * range);
            }

            public double NextDouble()
            {
                return _values.Count > 0 ? _values.Dequeue() : 0.0;
            }
        }
    }
}
