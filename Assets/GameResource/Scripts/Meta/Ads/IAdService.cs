using Cysharp.Threading.Tasks;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// AdMob·개발 스텁 등 광고 SDK 연동 추상화.
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// SDK 초기화 완료 여부.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 보상형 광고 로드 여부.
        /// </summary>
        bool IsRewardedReady { get; }

        /// <summary>
        /// 전면 광고 로드 여부.
        /// </summary>
        bool IsInterstitialReady { get; }

        /// <summary>
        /// 광고 SDK를 초기화한다.
        /// </summary>
        UniTask<bool> InitializeAsync(AdConfigTable config);

        /// <summary>
        /// 보상형 광고를 표시한다. 보상은 Completed 일 때만 지급한다.
        /// </summary>
        UniTask<AdShowOutcome> ShowRewardedAsync(string placementId);

        /// <summary>
        /// 전면 광고를 표시한다.
        /// </summary>
        UniTask<AdShowOutcome> ShowInterstitialAsync();
    }
}
