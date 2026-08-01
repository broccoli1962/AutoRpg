using Backend.Util.Localization;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// 스토어 준수 UI 문구. GSSL 키 추가 전까지 언어별 폴백을 제공한다.
    /// </summary>
    public static class StoreComplianceCopy
    {
        /// <summary>
        /// 설정·법적 고지 패널 제목.
        /// </summary>
        public static string PanelTitle => Resolve(
            "store.legal.title",
            "설정 및 법적 고지",
            "Settings & Legal",
            "設定と法的情報");

        /// <summary>
        /// 개인정보처리방침 버튼.
        /// </summary>
        public static string PrivacyPolicy => Resolve(
            "store.legal.privacy",
            "개인정보처리방침",
            "Privacy Policy",
            "プライバシーポリシー");

        /// <summary>
        /// 이용약관 버튼.
        /// </summary>
        public static string TermsOfService => Resolve(
            "store.legal.terms",
            "이용약관",
            "Terms of Service",
            "利用規約");

        /// <summary>
        /// 계정 삭제 요청 버튼.
        /// </summary>
        public static string AccountDeletion => Resolve(
            "store.legal.account_delete",
            "계정 삭제 요청",
            "Request Account Deletion",
            "アカウント削除申請");

        /// <summary>
        /// 확률 정보 버튼.
        /// </summary>
        public static string GachaRateInfo => Resolve(
            "store.legal.gacha_rate",
            "확률형 아이템 정보",
            "Probabilistic Item Info",
            "確率型アイテム情報");

        /// <summary>
        /// 청소년 결제 한도 안내.
        /// </summary>
        public static string TeenPaymentNotice => Resolve(
            "store.legal.teen_payment",
            "만 19세 미만 이용자의 경우 법정대리인 동의 또는 결제 한도가 적용될 수 있습니다. " +
            "자세한 내용은 이용약관 및 각 앱스토어 정책을 확인해 주세요.",
            "Users under 19 may be subject to guardian consent or purchase limits. " +
            "See the Terms of Service and store policies for details.",
            "19歳未満の利用者には法定代理人の同意または決済上限が適用される場合があります。" +
            "詳細は利用規約および各ストアのポリシーをご確認ください。");

        /// <summary>
        /// 아동 대상 아님 안내.
        /// </summary>
        public static string NotTargetingChildren => Resolve(
            "store.legal.not_children",
            "본 게임은 아동을 대상으로 하지 않습니다 (만 12세 이용가 기준).",
            "This game is not directed at children (rated 12+).",
            "本ゲームは児童を対象としていません（12+想定）。");

        /// <summary>
        /// 광고 동의 상태 — 허용.
        /// </summary>
        public static string AdConsentGranted => Resolve(
            "store.legal.ad_consent_granted",
            "광고 식별자: 허용됨",
            "Ad identifier: Allowed",
            "広告識別子: 許可");

        /// <summary>
        /// 광고 동의 상태 — 거부.
        /// </summary>
        public static string AdConsentDenied => Resolve(
            "store.legal.ad_consent_denied",
            "광고 식별자: 거부됨 (비개인화 광고)",
            "Ad identifier: Denied (non-personalized ads)",
            "広告識別子: 拒否（非パーソナライズ広告）");

        /// <summary>
        /// 광고 동의 재요청 버튼.
        /// </summary>
        public static string AdConsentManage => Resolve(
            "store.legal.ad_consent_manage",
            "광고 개인정보 설정",
            "Ad Privacy Settings",
            "広告プライバシー設定");

        /// <summary>
        /// 닫기 버튼.
        /// </summary>
        public static string Close => Resolve(
            "ui.common.close",
            "닫기",
            "Close",
            "閉じる");

        private static string Resolve(string key, string ko, string en, string ja)
        {
            var localized = LocalizationService.Get(key);
            if (!string.IsNullOrEmpty(localized) && localized != key && !localized.StartsWith("!"))
                return localized;

            return LocalizationService.CurrentLanguage switch
            {
                GameLanguage.Korean => ko,
                GameLanguage.Japanese => ja,
                _ => en,
            };
        }
    }
}
