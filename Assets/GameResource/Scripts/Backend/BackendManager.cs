using Backend.Services.Analytics;
using Backend.Services.Auth;
using Backend.Services.CrashReporting;
using Backend.Services.Push;
using Backend.Services.RemoteConfig;
using Backend.Services.Save;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Services
{
    /// <summary>
    /// Firebase BaaS 런타임 매니저. 인증·세이브·Remote Config·분석·Crashlytics·FCM 진입점.
    /// </summary>
    public sealed class BackendManager : SingletonGameObject<BackendManager>
    {
        private BackendServicesBundle _services;
        private bool _isBootstrapped;

        /// <summary>
        /// 백엔드 서비스 묶음.
        /// </summary>
        public BackendServicesBundle Services
        {
            get
            {
                EnsureBootstrapped();
                return _services;
            }
        }

        /// <summary>
        /// 백엔드 서비스를 초기화한다.
        /// </summary>
        public static UniTask<bool> BootstrapAsync()
        {
            if (GameStateUtil.IsQuitting)
                return UniTask.FromResult(false);

            return Instance.InitializeAsync();
        }

        /// <summary>
        /// 백엔드 서비스를 초기화한다.
        /// </summary>
        public async UniTask<bool> InitializeAsync(
            IAuthService authService = null,
            IRemoteConfigService remoteConfigService = null,
            ICloudSaveStore cloudSaveStore = null,
            ILocalSaveStore localSaveStore = null,
            IGameSaveAggregator saveAggregator = null,
            ISaveConflictPresenter conflictPresenter = null,
            IGameAnalyticsService analyticsService = null,
            ICrashReportingService crashReportingService = null,
            IPushNotificationService pushService = null)
        {
            if (GameStateUtil.IsQuitting)
                return false;

            _services?.AnalyticsBridge?.Dispose();

            authService ??= CreateDefaultAuthService();
            remoteConfigService ??= CreateDefaultRemoteConfigService();
            cloudSaveStore ??= CreateDefaultCloudSaveStore();
            localSaveStore ??= new EncryptedLocalSaveStore();
            saveAggregator ??= new GameSaveAggregator();
            analyticsService ??= CreateDefaultAnalyticsService();
            crashReportingService ??= CreateDefaultCrashReportingService();
            pushService ??= CreateDefaultPushService();

            var signedIn = await authService.InitializeAndSignInAnonymouslyAsync();
            if (!signedIn)
            {
                authService = new SimulatedAuthService();
                signedIn = await authService.InitializeAndSignInAnonymouslyAsync();
            }
            crashReportingService.Initialize();
            analyticsService.Initialize();

            if (signedIn && authService.CurrentUser != null)
            {
                crashReportingService.SetUserId(authService.CurrentUser.UserId);
                analyticsService.SetUserProperty("auth_provider", authService.CurrentUser.Provider.ToString());
            }

            await remoteConfigService.InitializeAndFetchAsync();
            var remoteConfigBinder = new RemoteConfigBinder(remoteConfigService);
            remoteConfigBinder.ApplyAll();

            var saveBackup = new SaveBackupService(
                authService,
                localSaveStore,
                cloudSaveStore,
                saveAggregator,
                conflictPresenter);

            await saveBackup.SynchronizeOnBootAsync();
            await pushService.InitializeAsync();

            var analyticsBridge = new GameplayAnalyticsBridge(analyticsService);
            analyticsBridge.Subscribe();

            _services = new BackendServicesBundle(
                authService,
                remoteConfigService,
                remoteConfigBinder,
                saveBackup,
                analyticsService,
                analyticsBridge,
                crashReportingService,
                pushService);

            _isBootstrapped = true;
            return true;
        }

        /// <summary>
        /// 현재 세이브를 로컬·클라우드에 저장한다.
        /// </summary>
        public static UniTask<bool> SaveAsync(bool forceCloudBackup = false)
        {
            if (GameStateUtil.IsQuitting || Instance == null || Instance._services?.SaveBackup == null)
                return UniTask.FromResult(false);

            return Instance._services.SaveBackup.SaveLocalAsync(forceCloudBackup);
        }

        /// <summary>
        /// Google 계정을 연동한다.
        /// </summary>
        public static UniTask<bool> LinkGoogleAsync(string idToken)
        {
            if (GameStateUtil.IsQuitting || Instance == null || Instance._services?.Auth == null)
                return UniTask.FromResult(false);

            return Instance._services.Auth.LinkGoogleAsync(idToken);
        }

        /// <summary>
        /// Apple 계정을 연동한다.
        /// </summary>
        public static UniTask<bool> LinkAppleAsync(string idToken, string rawNonce)
        {
            if (GameStateUtil.IsQuitting || Instance == null || Instance._services?.Auth == null)
                return UniTask.FromResult(false);

            return Instance._services.Auth.LinkAppleAsync(idToken, rawNonce);
        }

        /// <summary>
        /// 클라우드 세이브를 복원한다.
        /// </summary>
        public static UniTask<bool> RestoreFromCloudAsync()
        {
            if (GameStateUtil.IsQuitting || Instance == null || Instance._services?.SaveBackup == null)
                return UniTask.FromResult(false);

            return Instance._services.SaveBackup.RestoreFromCloudAsync();
        }

        /// <summary>
        /// 테스트용 서비스를 주입한다.
        /// </summary>
        public static void SetForTests(BackendServicesBundle services)
        {
            if (Instance == null)
                return;

            Instance._services = services;
            Instance._isBootstrapped = services != null;
        }

        private void EnsureBootstrapped()
        {
            if (_services != null)
                return;

            if (!_isBootstrapped)
                InitializeAsync().Forget();
        }

        private static IAuthService CreateDefaultAuthService()
        {
#if UNITY_EDITOR
            return new SimulatedAuthService();
#else
            var firebase = new FirebaseAuthService();
            return firebase;
#endif
        }

        private static IRemoteConfigService CreateDefaultRemoteConfigService()
        {
#if UNITY_EDITOR
            return new SimulatedRemoteConfigService();
#else
            return new FirebaseRemoteConfigService();
#endif
        }

        private static ICloudSaveStore CreateDefaultCloudSaveStore()
        {
#if UNITY_EDITOR
            return new SimulatedCloudSaveStore();
#else
            return new FirebaseCloudSaveStore();
#endif
        }

        private static IGameAnalyticsService CreateDefaultAnalyticsService()
        {
#if UNITY_EDITOR
            return new SimulatedAnalyticsService();
#else
            return new FirebaseAnalyticsService();
#endif
        }

        private static ICrashReportingService CreateDefaultCrashReportingService()
        {
#if UNITY_EDITOR
            return new NullCrashReportingService();
#else
            return new FirebaseCrashlyticsService();
#endif
        }

        private static IPushNotificationService CreateDefaultPushService()
        {
#if UNITY_EDITOR
            return new NullPushNotificationService();
#else
            return new FirebaseCloudMessagingService();
#endif
        }
    }
}
