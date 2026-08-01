using Backend.Services.Analytics;
using Backend.Services.Auth;
using Backend.Services.CrashReporting;
using Backend.Services.Push;
using Backend.Services.RemoteConfig;
using Backend.Services.Save;

namespace Backend.Services
{
    /// <summary>
    /// 백엔드 서비스 묶음.
    /// </summary>
    public sealed class BackendServicesBundle
    {
        public BackendServicesBundle(
            IAuthService auth,
            IRemoteConfigService remoteConfig,
            RemoteConfigBinder remoteConfigBinder,
            SaveBackupService saveBackup,
            IGameAnalyticsService analytics,
            GameplayAnalyticsBridge analyticsBridge,
            ICrashReportingService crashReporting,
            IPushNotificationService push)
        {
            Auth = auth;
            RemoteConfig = remoteConfig;
            RemoteConfigBinder = remoteConfigBinder;
            SaveBackup = saveBackup;
            Analytics = analytics;
            AnalyticsBridge = analyticsBridge;
            CrashReporting = crashReporting;
            Push = push;
        }

        public IAuthService Auth { get; }
        public IRemoteConfigService RemoteConfig { get; }
        public RemoteConfigBinder RemoteConfigBinder { get; }
        public SaveBackupService SaveBackup { get; }
        public IGameAnalyticsService Analytics { get; }
        public GameplayAnalyticsBridge AnalyticsBridge { get; }
        public ICrashReportingService CrashReporting { get; }
        public IPushNotificationService Push { get; }
    }
}
