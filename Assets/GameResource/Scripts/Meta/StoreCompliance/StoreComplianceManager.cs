using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// 스토어 준수·광고 동의 런타임 매니저.
    /// </summary>
    public sealed class StoreComplianceManager : SingletonGameObject<StoreComplianceManager>
    {
        private StoreComplianceService _complianceService;
        private IAdConsentService _adConsentService;
        private bool _isBootstrapped;

        /// <summary>
        /// 스토어 준수 서비스.
        /// </summary>
        public StoreComplianceService ComplianceService
        {
            get
            {
                EnsureBootstrapped();
                return _complianceService;
            }
        }

        /// <summary>
        /// 광고 동의 서비스.
        /// </summary>
        public IAdConsentService AdConsent
        {
            get
            {
                EnsureBootstrapped();
                return _adConsentService;
            }
        }

        /// <summary>
        /// 광고 동의 절차 완료 여부.
        /// </summary>
        public static bool IsAdConsentResolved()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return false;

            return Instance.AdConsent.IsResolved;
        }

        /// <summary>
        /// 개인화 광고 요청 가능 여부.
        /// </summary>
        public static bool CanRequestPersonalizedAds()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return false;

            return Instance.AdConsent.CanRequestPersonalizedAds;
        }

        /// <summary>
        /// 개인정보처리방침 페이지를 연다.
        /// </summary>
        public static void OpenPrivacyPolicy()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return;

            Instance.ComplianceService.OpenPrivacyPolicy();
        }

        /// <summary>
        /// 이용약관 페이지를 연다.
        /// </summary>
        public static void OpenTermsOfService()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return;

            Instance.ComplianceService.OpenTermsOfService();
        }

        /// <summary>
        /// 계정 삭제 요청 경로를 연다.
        /// </summary>
        public static void OpenAccountDeletionRequest()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return;

            Instance.ComplianceService.OpenAccountDeletionRequest();
        }

        /// <summary>
        /// 스토어 준수·광고 동의를 초기화한다.
        /// </summary>
        public static UniTask<bool> InitializeAsync(
            StoreComplianceService complianceService = null,
            IAdConsentService adConsentService = null)
        {
            if (GameStateUtil.IsQuitting)
                return UniTask.FromResult(false);

            return Instance.InitializeInternalAsync(complianceService, adConsentService);
        }

        /// <summary>
        /// 광고 동의 UI를 다시 요청한다.
        /// </summary>
        public static UniTask<AdConsentStatus> RequestAdConsentAsync()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return UniTask.FromResult(AdConsentStatus.Unknown);

            return Instance.AdConsent.RequestConsentAsync();
        }

        /// <summary>
        /// 테스트용 서비스를 주입한다.
        /// </summary>
        public static void SetForTests(
            StoreComplianceService complianceService,
            IAdConsentService adConsentService)
        {
            if (Instance == null)
                return;

            Instance._complianceService = complianceService;
            Instance._adConsentService = adConsentService;
            Instance._isBootstrapped = complianceService != null && adConsentService != null;
        }

        private async UniTask<bool> InitializeInternalAsync(
            StoreComplianceService complianceService,
            IAdConsentService adConsentService)
        {
            _complianceService = complianceService ?? new StoreComplianceService(StoreComplianceConfigProvider.Get());
            _adConsentService = adConsentService ?? CreateDefaultAdConsentService();
            _isBootstrapped = true;

            await _adConsentService.RequestConsentAsync();
            return true;
        }

        private void EnsureBootstrapped()
        {
            if (_complianceService != null && _adConsentService != null)
                return;

            if (!_isBootstrapped)
                InitializeInternalAsync(null, null).Forget();
        }

        private static IAdConsentService CreateDefaultAdConsentService()
        {
#if UNITY_EDITOR
            return new SimulatedAdConsentService();
#else
            return new PlatformAdConsentService();
#endif
        }
    }
}
