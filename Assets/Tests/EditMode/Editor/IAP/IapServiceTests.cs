using System;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Meta.IAP;
using Backend.Meta.Shop;
using Backend.Simulation;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.IAP.Tests
{
    public class IapServiceTests
    {
        private TransactionLedger _ledger;
        private Wallet _wallet;
        private ExplorerCatalog _catalog;
        private BalanceTable _balance;
        private ShopCatalogTable _catalogTable;
        private ShopService _shopService;
        private SimulatedIapStoreBridge _storeBridge;
        private IapService _iapService;

        [SetUp]
        public void SetUp()
        {
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _catalog = new ExplorerCatalog();
            _balance = ScriptableObject.CreateInstance<BalanceTable>();
            _catalogTable = ScriptableObject.CreateInstance<ShopCatalogTable>();
            _catalogTable.ApplySpecDefaults();
            _shopService = new ShopService(_wallet, _catalog, _balance);
            _storeBridge = new SimulatedIapStoreBridge();

            _iapService = new IapService(
                _shopService,
                _catalogTable,
                new LocalStubPurchaseValidator(),
                _storeBridge);
        }

        [TearDown]
        public void TearDown()
        {
            if (_catalogTable != null)
                UnityEngine.Object.DestroyImmediate(_catalogTable);

            if (_balance != null)
                UnityEngine.Object.DestroyImmediate(_balance);
        }

        [Test]
        public void PurchaseAsync_DoesNotGrantBeforeValidation()
        {
            var failingValidator = new FailingPurchaseValidator();
            var service = new IapService(
                _shopService,
                _catalogTable,
                failingValidator,
                _storeBridge);

            service.InitializeStoreAsync().GetAwaiter().GetResult();

            var result = service.PurchaseAsync("abyss_stone_1").GetAwaiter().GetResult();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.AbyssStone));
            Assert.AreEqual(1, service.PendingTransactionCount);
        }

        [Test]
        public void PurchaseAsync_GrantsAfterValidationSuccess()
        {
            _iapService.InitializeStoreAsync().GetAwaiter().GetResult();

            var result = _iapService.PurchaseAsync("abyss_stone_1").GetAwaiter().GetResult();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(600L, _wallet.GetBalance(CurrencyType.AbyssStone));
        }

        [Test]
        public void ProcessPendingTransactionsAsync_FulfillsQueuedPurchases()
        {
            var toggleValidator = new TogglePurchaseValidator(false);
            var service = new IapService(
                _shopService,
                _catalogTable,
                toggleValidator,
                _storeBridge);

            service.InitializeStoreAsync().GetAwaiter().GetResult();
            Assert.IsFalse(service.PurchaseAsync("abyss_stone_2").GetAwaiter().GetResult().Success);
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.AbyssStone));

            toggleValidator.AllowValidation = true;
            var processed = service.ProcessPendingTransactionsAsync().GetAwaiter().GetResult();

            Assert.AreEqual(1, processed);
            Assert.AreEqual(2200L, _wallet.GetBalance(CurrencyType.AbyssStone));
        }

        [Test]
        public void RestorePurchasesAsync_RestoresOwnedNonConsumables()
        {
            _iapService.InitializeStoreAsync().GetAwaiter().GetResult();
            _iapService.PurchaseAsync("ad_removal").GetAwaiter().GetResult();
            Assert.IsTrue(_shopService.HasPermanentAdRemoval);

            var freshShop = new ShopService(_wallet, _catalog, _balance);
            var restoredService = new IapService(
                freshShop,
                _catalogTable,
                new LocalStubPurchaseValidator(),
                _storeBridge);

            restoredService.InitializeStoreAsync().GetAwaiter().GetResult();
            var results = restoredService.RestorePurchasesAsync().GetAwaiter().GetResult();

            Assert.IsNotEmpty(results);
            Assert.IsTrue(freshShop.HasPermanentAdRemoval);
        }

        private sealed class FailingPurchaseValidator : IPurchaseValidator
        {
            public UniTask<PurchaseValidationResult> ValidateAsync(PurchaseValidationRequest request)
            {
                return UniTask.FromResult(PurchaseValidationResult.Failed("Validation rejected."));
            }
        }

        private sealed class TogglePurchaseValidator : IPurchaseValidator
        {
            public bool AllowValidation { get; set; }

            public TogglePurchaseValidator(bool allowValidation)
            {
                AllowValidation = allowValidation;
            }

            public UniTask<PurchaseValidationResult> ValidateAsync(PurchaseValidationRequest request)
            {
                return UniTask.FromResult(
                    AllowValidation
                        ? PurchaseValidationResult.Succeeded()
                        : PurchaseValidationResult.Failed("Validation rejected."));
            }
        }
    }
}
