using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// Android UMP·iOS ATT 연동 구현체. SDK define 없으면 NotRequired 로 폴백한다.
    /// </summary>
    public sealed class PlatformAdConsentService : IAdConsentService
    {
        private AdConsentStatus _status = AdConsentStatus.Unknown;
        private bool _isResolved;

        /// <summary>
        /// 동의 절차 완료 여부.
        /// </summary>
        public bool IsResolved => _isResolved;

        /// <summary>
        /// 현재 동의 상태.
        /// </summary>
        public AdConsentStatus Status => _status;

        /// <summary>
        /// 개인화 광고 요청 가능 여부.
        /// </summary>
        public bool CanRequestPersonalizedAds =>
            _status == AdConsentStatus.Obtained || _status == AdConsentStatus.NotRequired;

        /// <summary>
        /// 플랫폼별 동의 UI를 요청한다.
        /// </summary>
        public async UniTask<AdConsentStatus> RequestConsentAsync()
        {
#if UNITY_ANDROID && ABYSS_HAS_GOOGLE_MOBILE_ADS
            _status = await RequestUmpConsentAsync();
#elif UNITY_IOS && ABYSS_HAS_IOS_ATT
            _status = await RequestAttConsentAsync();
#else
            _status = AdConsentStatus.NotRequired;
#endif
            _isResolved = true;
            return _status;
        }

#if UNITY_ANDROID && ABYSS_HAS_GOOGLE_MOBILE_ADS
        private static async UniTask<AdConsentStatus> RequestUmpConsentAsync()
        {
            var tcs = new UniTaskCompletionSource<AdConsentStatus>();

            var request = new GoogleMobileAds.Ump.Api.ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
            };

            GoogleMobileAds.Ump.Api.ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null)
                {
                    tcs.TrySetResult(AdConsentStatus.NotRequired);
                    return;
                }

                GoogleMobileAds.Ump.Api.ConsentForm.LoadAndShowConsentFormIfRequired(showError =>
                {
                    if (showError != null)
                    {
                        tcs.TrySetResult(AdConsentStatus.Denied);
                        return;
                    }

                    var canRequest = GoogleMobileAds.Ump.Api.ConsentInformation.CanRequestAds();
                    tcs.TrySetResult(canRequest ? AdConsentStatus.Obtained : AdConsentStatus.Denied);
                });
            });

            return await tcs.Task;
        }
#endif

#if UNITY_IOS && ABYSS_HAS_IOS_ATT
        private static async UniTask<AdConsentStatus> RequestAttConsentAsync()
        {
            var currentStatus = Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (currentStatus != Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                return currentStatus == Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED
                    ? AdConsentStatus.Obtained
                    : AdConsentStatus.Denied;
            }

            Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTrackingStatus();
            await UniTask.Delay(500);

            var status = Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            return status == Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED
                ? AdConsentStatus.Obtained
                : AdConsentStatus.Denied;
        }
#endif
    }
}
