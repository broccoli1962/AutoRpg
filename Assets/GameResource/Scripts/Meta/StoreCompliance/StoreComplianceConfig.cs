using UnityEngine;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// 스토어 준수 링크·문구 ScriptableObject. 환경별 URL은 빌드 파이프라인에서 주입한다.
    /// </summary>
    [CreateAssetMenu(fileName = "StoreComplianceConfig", menuName = "Abyss Chronicle/Store Compliance Config")]
    public sealed class StoreComplianceConfig : ScriptableObject
    {
        [SerializeField] private string _privacyPolicyUrl = "https://example.com/abyss-chronicle/privacy";
        [SerializeField] private string _termsOfServiceUrl = "https://example.com/abyss-chronicle/terms";
        [SerializeField] private string _accountDeletionUrl = "https://example.com/abyss-chronicle/account-delete";
        [SerializeField] private string _accountDeletionEmail = "privacy@example.com";

        /// <summary>
        /// 개인정보처리방침 URL.
        /// </summary>
        public string PrivacyPolicyUrl => _privacyPolicyUrl;

        /// <summary>
        /// 이용약관 URL.
        /// </summary>
        public string TermsOfServiceUrl => _termsOfServiceUrl;

        /// <summary>
        /// 계정 삭제 요청 웹 URL.
        /// </summary>
        public string AccountDeletionUrl => _accountDeletionUrl;

        /// <summary>
        /// 계정 삭제 요청 이메일 (mailto 폴백).
        /// </summary>
        public string AccountDeletionEmail => _accountDeletionEmail;

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _privacyPolicyUrl = "https://example.com/abyss-chronicle/privacy";
            _termsOfServiceUrl = "https://example.com/abyss-chronicle/terms";
            _accountDeletionUrl = "https://example.com/abyss-chronicle/account-delete";
            _accountDeletionEmail = "privacy@example.com";
        }
    }
}
