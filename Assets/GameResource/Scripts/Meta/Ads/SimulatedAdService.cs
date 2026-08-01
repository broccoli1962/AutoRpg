using System;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// EditMode·SDK 없는 개발 빌드용 광고 스텁.
    /// </summary>
    public sealed class SimulatedAdService : IAdService
    {
        private AdShowOutcome _rewardedOutcome = AdShowOutcome.Completed;
        private AdShowOutcome _interstitialOutcome = AdShowOutcome.Completed;
        private bool _rewardedReady = true;
        private bool _interstitialReady = true;

        /// <summary>
        /// SDK 초기화 완료 여부.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 보상형 광고 로드 여부.
        /// </summary>
        public bool IsRewardedReady => IsInitialized && _rewardedReady;

        /// <summary>
        /// 전면 광고 로드 여부.
        /// </summary>
        public bool IsInterstitialReady => IsInitialized && _interstitialReady;

        /// <summary>
        /// 광고 SDK를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeAsync(AdConfigTable config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            IsInitialized = true;
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 보상형 광고를 표시한다.
        /// </summary>
        public UniTask<AdShowOutcome> ShowRewardedAsync(string placementId)
        {
            if (!IsInitialized)
                return UniTask.FromResult(AdShowOutcome.Failed);

            if (!_rewardedReady)
                return UniTask.FromResult(AdShowOutcome.NotLoaded);

            return UniTask.FromResult(_rewardedOutcome);
        }

        /// <summary>
        /// 전면 광고를 표시한다.
        /// </summary>
        public UniTask<AdShowOutcome> ShowInterstitialAsync()
        {
            if (!IsInitialized)
                return UniTask.FromResult(AdShowOutcome.Failed);

            if (!_interstitialReady)
                return UniTask.FromResult(AdShowOutcome.NotLoaded);

            return UniTask.FromResult(_interstitialOutcome);
        }

        /// <summary>
        /// 테스트용 보상형 결과를 설정한다.
        /// </summary>
        public void SetRewardedOutcomeForTests(AdShowOutcome outcome, bool isReady = true)
        {
            _rewardedOutcome = outcome;
            _rewardedReady = isReady;
        }

        /// <summary>
        /// 테스트용 전면 결과를 설정한다.
        /// </summary>
        public void SetInterstitialOutcomeForTests(AdShowOutcome outcome, bool isReady = true)
        {
            _interstitialOutcome = outcome;
            _interstitialReady = isReady;
        }
    }
}
