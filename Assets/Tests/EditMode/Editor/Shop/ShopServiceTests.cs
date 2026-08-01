using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Meta.IAP;
using Backend.Meta.Shop;
using Backend.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.Shop.Tests
{
    public class ShopServiceTests
    {
        private TransactionLedger _ledger;
        private Wallet _wallet;
        private ExplorerCatalog _catalog;
        private BalanceTable _balance;
        private ShopCatalogTable _table;
        private ShopService _service;
        private LocalStubShopPurchaseStateSync _stateSync;
        private DateTimeOffset _nowUtc;

        [SetUp]
        public void SetUp()
        {
            _nowUtc = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _catalog = new ExplorerCatalog();
            _balance = ScriptableObject.CreateInstance<BalanceTable>();
            _table = ScriptableObject.CreateInstance<ShopCatalogTable>();
            _table.ApplySpecDefaults();
            _stateSync = new LocalStubShopPurchaseStateSync();

            _service = new ShopService(
                _wallet,
                _catalog,
                _balance,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc,
                _stateSync);
            _service.BindCurrentFloorProvider(() => 100);
        }

        [TearDown]
        public void TearDown()
        {
            if (_table != null)
                UnityEngine.Object.DestroyImmediate(_table);

            if (_balance != null)
                UnityEngine.Object.DestroyImmediate(_balance);
        }

        [Test]
        public void ShopCatalogTable_DefinesSixProductCategories()
        {
            var categories = new System.Collections.Generic.HashSet<ShopProductCategory>();

            foreach (var product in _table.Products)
                categories.Add(product.Category);

            Assert.IsTrue(categories.Contains(ShopProductCategory.StarterGrowthPack));
            Assert.IsTrue(categories.Contains(ShopProductCategory.AbyssStonePack));
            Assert.IsTrue(categories.Contains(ShopProductCategory.MonthlyAbyssContract));
            Assert.IsTrue(categories.Contains(ShopProductCategory.SeasonPass));
            Assert.IsTrue(categories.Contains(ShopProductCategory.AdRemoval));
            Assert.IsTrue(categories.Contains(ShopProductCategory.TieredGrowthPack));
            Assert.GreaterOrEqual(_table.Products.Count, 10);
        }

        [Test]
        public void FulfillValidatedPurchase_GrantsFirstPurchaseBonusOnce()
        {
            var product = _table.FindByProductId("abyss_stone_1");

            var first = _service.FulfillValidatedPurchase(product, "tx_1");

            Assert.IsTrue(first.Success);
            Assert.AreEqual(600L, _wallet.GetBalance(CurrencyType.AbyssStone));
            Assert.IsTrue(_service.IsFirstPurchaseBonusUsed("abyss_stone_1"));

            var second = _service.FulfillValidatedPurchase(product, "tx_2");
            Assert.IsTrue(second.Success);
            Assert.AreEqual(900L, _wallet.GetBalance(CurrencyType.AbyssStone));
        }

        [Test]
        public void FulfillValidatedPurchase_BlocksDuplicateTransaction()
        {
            var product = _table.FindByProductId("abyss_stone_2");

            Assert.IsTrue(_service.FulfillValidatedPurchase(product, "tx_dup").Success);
            Assert.IsFalse(_service.FulfillValidatedPurchase(product, "tx_dup").Success);
        }

        [Test]
        public void FulfillValidatedPurchase_BlocksOneTimeProductTwice()
        {
            var product = _table.FindByProductId("starter_growth_pack");

            Assert.IsTrue(_service.FulfillValidatedPurchase(product, "tx_starter_1").Success);
            Assert.IsFalse(_service.CanPurchase(product));
            Assert.IsFalse(_service.FulfillValidatedPurchase(product, "tx_starter_2").Success);
        }

        [Test]
        public void SubscriptionExpiry_UsesServerTime()
        {
            var product = _table.FindByProductId("monthly_abyss_contract");
            var expiry = _nowUtc.AddDays(10);

            _service.FulfillValidatedPurchase(product, "tx_sub", expiry);

            Assert.IsTrue(_service.HasActiveMonthlyContract);

            _nowUtc = expiry.AddHours(1);
            _service.RefreshSubscriptionState();

            Assert.IsFalse(_service.HasActiveMonthlyContract);
        }

        [Test]
        public void PurchaseStateSync_PersistsConsumedAndBonusFlags()
        {
            var product = _table.FindByProductId("starter_growth_pack");
            _service.FulfillValidatedPurchase(product, "tx_sync");

            var reloaded = ShopService.FromSaveData(
                _service.ToSaveData(),
                _wallet,
                _catalog,
                _balance,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc,
                _stateSync);

            Assert.IsTrue(reloaded.IsOneTimeProductConsumed("starter_growth_pack"));
            Assert.IsTrue(_stateSync.TryRestorePurchaseState(out var serverState));
            Assert.Contains("starter_growth_pack", serverState.ConsumedOneTimeProductIds);
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
