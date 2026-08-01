using UnityEngine;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// 스토어 준수 링크·계정 삭제·외부 URL 오픈.
    /// </summary>
    public sealed class StoreComplianceService
    {
        private readonly StoreComplianceConfig _config;

        /// <summary>
        /// StoreComplianceService 를 생성한다.
        /// </summary>
        public StoreComplianceService(StoreComplianceConfig config)
        {
            _config = config ?? StoreComplianceConfigProvider.Get();
        }

        /// <summary>
        /// 개인정보처리방침 페이지를 연다.
        /// </summary>
        public void OpenPrivacyPolicy()
        {
            OpenUrl(_config.PrivacyPolicyUrl);
        }

        /// <summary>
        /// 이용약관 페이지를 연다.
        /// </summary>
        public void OpenTermsOfService()
        {
            OpenUrl(_config.TermsOfServiceUrl);
        }

        /// <summary>
        /// 계정 삭제 요청 경로를 연다. URL이 없으면 mailto 로 폴백한다.
        /// </summary>
        public void OpenAccountDeletionRequest()
        {
            if (!string.IsNullOrEmpty(_config.AccountDeletionUrl))
            {
                OpenUrl(_config.AccountDeletionUrl);
                return;
            }

            if (!string.IsNullOrEmpty(_config.AccountDeletionEmail))
                OpenUrl($"mailto:{_config.AccountDeletionEmail}?subject=Account%20Deletion%20Request");
        }

        /// <summary>
        /// 외부 URL을 연다.
        /// </summary>
        public static void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            Application.OpenURL(url);
        }
    }
}
