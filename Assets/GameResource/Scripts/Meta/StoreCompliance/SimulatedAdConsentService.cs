using Cysharp.Threading.Tasks;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// EditMode·SDK 없는 개발 빌드용 광고 동의 스텁.
    /// </summary>
    public sealed class SimulatedAdConsentService : IAdConsentService
    {
        private AdConsentStatus _status = AdConsentStatus.NotRequired;

        /// <summary>
        /// 동의 절차 완료 여부.
        /// </summary>
        public bool IsResolved { get; private set; }

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
        /// 즉시 NotRequired 로 해결한다.
        /// </summary>
        public UniTask<AdConsentStatus> RequestConsentAsync()
        {
            _status = AdConsentStatus.NotRequired;
            IsResolved = true;
            return UniTask.FromResult(_status);
        }

        /// <summary>
        /// 테스트용 상태를 주입한다.
        /// </summary>
        public void SetForTests(AdConsentStatus status, bool isResolved = true)
        {
            _status = status;
            IsResolved = isResolved;
        }
    }
}
