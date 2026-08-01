using Backend.Meta.Achievements;
using Backend.Meta.Ads;
using Backend.Simulation;

namespace Backend.Services.RemoteConfig
{
    /// <summary>
    /// Remote Config 값을 밸런스·광고·업적·이벤트 토글에 바인딩한다.
    /// </summary>
    public sealed class RemoteConfigBinder
    {
        private readonly IRemoteConfigService _remoteConfig;
        private readonly AchievementRemoteConfigOverlay _achievementOverlay;

        public RemoteConfigBinder(
            IRemoteConfigService remoteConfig,
            AchievementRemoteConfigOverlay achievementOverlay = null)
        {
            _remoteConfig = remoteConfig;
            _achievementOverlay = achievementOverlay ?? new AchievementRemoteConfigOverlay();
        }

        /// <summary>
        /// 업적 Remote Config 오버레이.
        /// </summary>
        public AchievementRemoteConfigOverlay AchievementOverlay => _achievementOverlay;

        /// <summary>
        /// Remote Config 값을 런타임 테이블에 적용한다.
        /// </summary>
        public void ApplyAll()
        {
            if (_remoteConfig == null || !_remoteConfig.IsReady)
                return;

            ApplyBalanceOverrides();
            ApplyAdOverrides();
            _achievementOverlay.ApplyRemoteValues(_remoteConfig.GetAllValues());
        }

        /// <summary>
        /// 이벤트 on/off 플래그를 조회한다.
        /// </summary>
        public bool IsEventEnabled(string key, bool defaultValue = true)
        {
            return _remoteConfig?.GetBool(key, defaultValue) ?? defaultValue;
        }

        private void ApplyBalanceOverrides()
        {
            var table = BalanceTableProvider.Get();
            table.ApplyRemoteOverrides(_remoteConfig.GetAllValues());
        }

        private void ApplyAdOverrides()
        {
            var table = AdConfigTableProvider.Get();
            table.ApplyRemoteOverrides(_remoteConfig.GetAllValues());
        }
    }
}
