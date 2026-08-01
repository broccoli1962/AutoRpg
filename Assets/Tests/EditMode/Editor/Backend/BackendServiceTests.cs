using System;
using System.IO;
using Backend.Meta.Achievements;
using Backend.Services.Analytics;
using Backend.Services.Auth;
using Backend.Services.RemoteConfig;
using Backend.Services.Save;
using Backend.Simulation;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Services.Tests
{
    public class BackendServiceTests
    {
        private SimulatedAuthService _auth;
        private SimulatedRemoteConfigService _remoteConfig;
        private SimulatedAnalyticsService _analytics;
        private GameplayAnalyticsBridge _analyticsBridge;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("abyss_sim_auth_user_id");
            PlayerPrefs.DeleteKey("abyss_sim_auth_provider");
            PlayerPrefs.DeleteKey("abyss_sim_auth_is_anonymous");
            PlayerPrefs.Save();

            var defaults = ScriptableObject.CreateInstance<RemoteConfigDefaultsTable>();
            defaults.ApplySpecDefaults();
            RemoteConfigDefaultsTableProvider.SetForTests(defaults);

            _auth = new SimulatedAuthService();
            _remoteConfig = new SimulatedRemoteConfigService(defaults);
            _analytics = new SimulatedAnalyticsService();
            _analytics.Initialize();
            _analyticsBridge = new GameplayAnalyticsBridge(_analytics);
            _analyticsBridge.Subscribe();
        }

        [TearDown]
        public void TearDown()
        {
            _analyticsBridge?.Dispose();
            BackendAnalyticsEvents.ClearSubscribers();
            MetaGameplayEvents.ClearSubscribers();
            RemoteConfigDefaultsTableProvider.ResetCache();
            BalanceTableProvider.ResetCache();
            Backend.Meta.Ads.AdConfigTableProvider.ResetCache();

            PlayerPrefs.DeleteKey("abyss_sim_auth_user_id");
            PlayerPrefs.DeleteKey("abyss_sim_auth_provider");
            PlayerPrefs.DeleteKey("abyss_sim_auth_is_anonymous");
            PlayerPrefs.Save();
        }

        [Test]
        public void SimulatedAuthService_SignsInAnonymouslyAndLinksGoogle()
        {
            _auth.InitializeAndSignInAnonymouslyAsync().GetAwaiter().GetResult();

            Assert.IsTrue(_auth.IsInitialized);
            Assert.IsNotNull(_auth.CurrentUser);
            Assert.IsTrue(_auth.CurrentUser.IsAnonymous);

            var linked = _auth.LinkGoogleAsync("test-token").GetAwaiter().GetResult();
            Assert.IsTrue(linked);
            Assert.IsFalse(_auth.CurrentUser.IsAnonymous);
            Assert.AreEqual(AuthLinkProvider.Google, _auth.CurrentUser.Provider);
        }

        [Test]
        public void SimulatedAuthService_LinksApple()
        {
            _auth.InitializeAndSignInAnonymouslyAsync().GetAwaiter().GetResult();

            var linked = _auth.LinkAppleAsync("apple-token", "nonce").GetAwaiter().GetResult();
            Assert.IsTrue(linked);
            Assert.AreEqual(AuthLinkProvider.Apple, _auth.CurrentUser.Provider);
        }

        [Test]
        public void SimulatedRemoteConfigService_UsesBundleDefaultsOnFetchFailure()
        {
            _remoteConfig.SimulateFetchFailureAsync().GetAwaiter().GetResult();

            Assert.IsTrue(_remoteConfig.IsReady);
            Assert.IsFalse(_remoteConfig.LastFetchSucceeded);
            Assert.AreEqual(1.135d, _remoteConfig.GetDouble(RemoteConfigKeys.MonsterHpGrowth, 0d), 0.0001d);
            Assert.AreEqual(15, (int)_remoteConfig.GetDouble(RemoteConfigKeys.TotalRewardedDailyLimit, 0d));
        }

        [Test]
        public void RemoteConfigBinder_AppliesBalanceAndAdOverrides()
        {
            _remoteConfig.InitializeAndFetchAsync().GetAwaiter().GetResult();
            _remoteConfig.SetRemoteValueForTests(RemoteConfigKeys.MonsterHpGrowth, "1.200");
            _remoteConfig.SetRemoteValueForTests(RemoteConfigKeys.TotalRewardedDailyLimit, "20");

            var balance = ScriptableObject.CreateInstance<BalanceTable>();
            balance.ApplySpecDefaults();
            BalanceTableProvider.SetForTests(balance);

            var binder = new RemoteConfigBinder(_remoteConfig);
            binder.ApplyAll();

            Assert.AreEqual(1.200f, balance.MonsterHpGrowth, 0.0001f);

            var adConfig = ScriptableObject.CreateInstance<Backend.Meta.Ads.AdConfigTable>();
            adConfig.ApplySpecDefaults();
            Backend.Meta.Ads.AdConfigTableProvider.SetForTests(adConfig);
            binder.ApplyAll();
            Assert.AreEqual(20, adConfig.TotalRewardedDailyLimit);
        }

        [Test]
        public void GameplayAnalyticsBridge_LogsCoreEvents()
        {
            BackendAnalyticsEvents.ReportTutorialStep(3);
            MetaGameplayEvents.ReportFloorReached(42);
            MetaGameplayEvents.ReportPrestige();
            MetaGameplayEvents.ReportSummon(10);
            BackendAnalyticsEvents.ReportShopView();
            BackendAnalyticsEvents.ReportShopPurchase("pack_starter");
            BackendAnalyticsEvents.ReportAdWatch("offline_double");
            BackendAnalyticsEvents.ReportGrowthWall(50);

            Assert.AreEqual(8, _analytics.Records.Count);
            Assert.AreEqual(GameAnalyticsEvents.TutorialStep, _analytics.Records[0].EventName);
            Assert.AreEqual(GameAnalyticsEvents.FloorReached, _analytics.Records[1].EventName);
            Assert.AreEqual(GameAnalyticsEvents.Prestige, _analytics.Records[2].EventName);
            Assert.AreEqual(GameAnalyticsEvents.Summon, _analytics.Records[3].EventName);
            Assert.AreEqual(GameAnalyticsEvents.ShopView, _analytics.Records[4].EventName);
            Assert.AreEqual(GameAnalyticsEvents.ShopPurchase, _analytics.Records[5].EventName);
            Assert.AreEqual(GameAnalyticsEvents.AdWatch, _analytics.Records[6].EventName);
            Assert.AreEqual(GameAnalyticsEvents.GrowthWall, _analytics.Records[7].EventName);
        }
    }

    public class SaveBackupServiceTests
    {
        private const string TestSavePath = "abyss_save_test.dat";

        private SimulatedAuthService _auth;
        private SimulatedCloudSaveStore _cloud;
        private EncryptedLocalSaveStore _local;
        private GameSaveAggregator _aggregator;
        private SaveBackupService _service;
        private string _walletMarker;
        private DateTimeOffset _nowUtc;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("abyss_sim_auth_user_id");
            PlayerPrefs.DeleteKey("abyss_sim_auth_provider");
            PlayerPrefs.DeleteKey("abyss_sim_auth_is_anonymous");
            PlayerPrefs.Save();

            _nowUtc = new DateTimeOffset(2026, 8, 1, 4, 0, 0, TimeSpan.Zero);
            _auth = new SimulatedAuthService();
            _auth.InitializeAndSignInAnonymouslyAsync().GetAwaiter().GetResult();

            var path = Path.Combine(Application.persistentDataPath, TestSavePath);
            if (File.Exists(path))
                File.Delete(path);

            _local = new EncryptedLocalSaveStore(path);
            _cloud = new SimulatedCloudSaveStore();
            _walletMarker = "local-state";

            _aggregator = new GameSaveAggregator(
                exportWallet: () => new Backend.Meta.Currency.WalletSaveData
                {
                    Balances = new[]
                    {
                        new Backend.Meta.Currency.CurrencyBalanceEntry
                        {
                            Type = Backend.Meta.Currency.CurrencyType.Gold,
                            Amount = _walletMarker.GetHashCode(),
                        },
                    },
                },
                importWallet: saveData =>
                {
                    if (saveData?.Balances != null && saveData.Balances.Length > 0)
                        _walletMarker = saveData.Balances[0].Amount.ToString();
                });

            _service = new SaveBackupService(
                _auth,
                _local,
                _cloud,
                _aggregator,
                new AutoLocalSaveConflictPresenter(),
                () => _nowUtc);
        }

        [TearDown]
        public void TearDown()
        {
            var path = Path.Combine(Application.persistentDataPath, TestSavePath);
            if (File.Exists(path))
                File.Delete(path);
        }

        [Test]
        public void SaveBackupService_UploadsLocalToCloudWhenCloudMissing()
        {
            _service.SaveLocalAsync(forceCloudBackup: true).GetAwaiter().GetResult();

            var metadata = _cloud.FetchMetadataAsync(_auth.CurrentUser.UserId).GetAwaiter().GetResult();
            Assert.IsNotNull(metadata);
        }

        [Test]
        public void SaveBackupService_RestoresFromCloudWhenLocalMissing()
        {
            var snapshot = _aggregator.ExportSnapshot();
            _cloud.UploadAsync(_auth.CurrentUser.UserId, snapshot).GetAwaiter().GetResult();

            _walletMarker = "empty";
            _service.SynchronizeOnBootAsync().GetAwaiter().GetResult();

            Assert.AreNotEqual("empty", _walletMarker);
        }

        [Test]
        public void SaveBackupService_RestoreFromCloudAsync_AppliesSnapshot()
        {
            _walletMarker = "before";
            _service.SaveLocalAsync(forceCloudBackup: true).GetAwaiter().GetResult();

            _walletMarker = "cleared";
            var restored = _service.RestoreFromCloudAsync().GetAwaiter().GetResult();

            Assert.IsTrue(restored);
            Assert.AreNotEqual("cleared", _walletMarker);
        }
    }
}
