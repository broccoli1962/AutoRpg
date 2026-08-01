using Cysharp.Threading.Tasks;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// ATT(iOS)·UMP(Android) 광고 식별자 동의 추상화.
    /// </summary>
    public interface IAdConsentService
    {
        /// <summary>
        /// 동의 절차가 완료되었는지 여부.
        /// </summary>
        bool IsResolved { get; }

        /// <summary>
        /// 현재 동의 상태.
        /// </summary>
        AdConsentStatus Status { get; }

        /// <summary>
        /// 개인화 광고 요청 가능 여부.
        /// </summary>
        bool CanRequestPersonalizedAds { get; }

        /// <summary>
        /// 플랫폼 동의 UI를 표시하고 결과를 반환한다.
        /// </summary>
        UniTask<AdConsentStatus> RequestConsentAsync();
    }
}
