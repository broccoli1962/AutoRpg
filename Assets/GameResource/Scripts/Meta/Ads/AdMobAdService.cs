using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// Google AdMob 연동 구현체. ABYSS_HAS_GOOGLE_MOBILE_ADS 정의 시 실 SDK를 사용한다.
    /// </summary>
    public sealed class AdMobAdService : MonoBehaviour, IAdService
    {
        private AdConfigTable _config;
        private bool _isInitialized;
        private bool _rewardedReady;
        private bool _interstitialReady;

#if ABYSS_HAS_GOOGLE_MOBILE_ADS
        private GoogleMobileAds.Api.RewardedAd _rewardedAd;
        private GoogleMobileAds.Api.InterstitialAd _interstitialAd;
#endif

        /// <summary>
        /// SDK 초기화 완료 여부.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 보상형 광고 로드 여부.
        /// </summary>
        public bool IsRewardedReady => _isInitialized && _rewardedReady;

        /// <summary>
        /// 전면 광고 로드 여부.
        /// </summary>
        public bool IsInterstitialReady => _isInitialized && _interstitialReady;

        /// <summary>
        /// 광고 SDK를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeAsync(AdConfigTable config)
        {
            _config = config;

#if ABYSS_HAS_GOOGLE_MOBILE_ADS
            return InitializeAdMobAsync();
#else
            _isInitialized = false;
            _rewardedReady = false;
            _interstitialReady = false;
            return UniTask.FromResult(false);
#endif
        }

        /// <summary>
        /// 보상형 광고를 표시한다.
        /// </summary>
        public UniTask<AdShowOutcome> ShowRewardedAsync(string placementId)
        {
#if ABYSS_HAS_GOOGLE_MOBILE_ADS
            return ShowRewardedAdMobAsync(placementId);
#else
            return UniTask.FromResult(AdShowOutcome.NotLoaded);
#endif
        }

        /// <summary>
        /// 전면 광고를 표시한다.
        /// </summary>
        public UniTask<AdShowOutcome> ShowInterstitialAsync()
        {
#if ABYSS_HAS_GOOGLE_MOBILE_ADS
            return ShowInterstitialAdMobAsync();
#else
            return UniTask.FromResult(AdShowOutcome.NotLoaded);
#endif
        }

#if ABYSS_HAS_GOOGLE_MOBILE_ADS
        private async UniTask<bool> InitializeAdMobAsync()
        {
            var tcs = new UniTaskCompletionSource<bool>();

            GoogleMobileAds.Api.MobileAds.Initialize(_ =>
            {
                _isInitialized = true;
                LoadRewardedAd();
                LoadInterstitialAd();
                tcs.TrySetResult(true);
            });

            return await tcs.Task;
        }

        private void LoadRewardedAd()
        {
            var adUnitId = _config?.RewardedAdUnitId;
            if (string.IsNullOrEmpty(adUnitId))
            {
                _rewardedReady = false;
                return;
            }

            GoogleMobileAds.Api.RewardedAd.Load(adUnitId, new GoogleMobileAds.Api.AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    _rewardedReady = false;
                    return;
                }

                _rewardedAd = ad;
                _rewardedReady = true;
            });
        }

        private void LoadInterstitialAd()
        {
            var adUnitId = _config?.InterstitialAdUnitId;
            if (string.IsNullOrEmpty(adUnitId))
            {
                _interstitialReady = false;
                return;
            }

            GoogleMobileAds.Api.InterstitialAd.Load(adUnitId, new GoogleMobileAds.Api.AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    _interstitialReady = false;
                    return;
                }

                _interstitialAd = ad;
                _interstitialReady = true;
            });
        }

        private async UniTask<AdShowOutcome> ShowRewardedAdMobAsync(string placementId)
        {
            if (!IsRewardedReady || _rewardedAd == null)
                return AdShowOutcome.NotLoaded;

            var tcs = new UniTaskCompletionSource<AdShowOutcome>();

            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                if (!tcs.Task.Status.IsCompleted())
                    tcs.TrySetResult(AdShowOutcome.Skipped);

                _rewardedAd = null;
                _rewardedReady = false;
                LoadRewardedAd();
            };

            _rewardedAd.OnAdFullScreenContentFailed += _ =>
            {
                tcs.TrySetResult(AdShowOutcome.Failed);
                _rewardedAd = null;
                _rewardedReady = false;
                LoadRewardedAd();
            };

            _rewardedAd.Show(_ =>
            {
                tcs.TrySetResult(AdShowOutcome.Completed);
            });

            return await tcs.Task;
        }

        private async UniTask<AdShowOutcome> ShowInterstitialAdMobAsync()
        {
            if (!IsInterstitialReady || _interstitialAd == null)
                return AdShowOutcome.NotLoaded;

            var tcs = new UniTaskCompletionSource<AdShowOutcome>();

            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                tcs.TrySetResult(AdShowOutcome.Completed);
                _interstitialAd = null;
                _interstitialReady = false;
                LoadInterstitialAd();
            };

            _interstitialAd.OnAdFullScreenContentFailed += _ =>
            {
                tcs.TrySetResult(AdShowOutcome.Failed);
                _interstitialAd = null;
                _interstitialReady = false;
                LoadInterstitialAd();
            };

            _interstitialAd.Show();
            return await tcs.Task;
        }
#endif
    }
}
