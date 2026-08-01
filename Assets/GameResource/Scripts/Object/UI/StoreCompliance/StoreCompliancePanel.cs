using Backend.Meta.StoreCompliance;
using Backend.Object.Management;
using Backend.Object.UI.Gacha;
using Backend.Util.Localization;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.StoreCompliance
{
    /// <summary>
    /// 스토어 준수·법적 고지 패널 View.
    /// </summary>
    public sealed class StoreCompliancePanel : UIPanel<StoreCompliancePresenter>
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;

        [Header("Notices")]
        [SerializeField] private Text _teenPaymentNoticeText;
        [SerializeField] private Text _notChildrenNoticeText;
        [SerializeField] private Text _adConsentStatusText;

        [Header("Actions")]
        [SerializeField] private CommonButton _privacyButton;
        [SerializeField] private CommonButton _termsButton;
        [SerializeField] private CommonButton _accountDeleteButton;
        [SerializeField] private CommonButton _gachaRateButton;
        [SerializeField] private CommonButton _adConsentButton;
        [SerializeField] private CommonButton _closeButton;

        public Text TeenPaymentNoticeText => _teenPaymentNoticeText;
        public Text NotChildrenNoticeText => _notChildrenNoticeText;
        public Text AdConsentStatusText => _adConsentStatusText;
        public CommonButton PrivacyButton => _privacyButton;
        public CommonButton TermsButton => _termsButton;
        public CommonButton AccountDeleteButton => _accountDeleteButton;
        public CommonButton GachaRateButton => _gachaRateButton;
        public CommonButton AdConsentButton => _adConsentButton;
        public CommonButton CloseButton => _closeButton;

        /// <summary>
        /// 제목을 설정한다.
        /// </summary>
        public void SetTitle(string text)
        {
            if (_titleText != null)
                _titleText.text = text;
        }

        /// <summary>
        /// 청소년 결제 안내 문구를 설정한다.
        /// </summary>
        public void SetTeenPaymentNotice(string text)
        {
            if (_teenPaymentNoticeText != null)
                _teenPaymentNoticeText.text = text;
        }

        /// <summary>
        /// 아동 대상 아님 안내를 설정한다.
        /// </summary>
        public void SetNotChildrenNotice(string text)
        {
            if (_notChildrenNoticeText != null)
                _notChildrenNoticeText.text = text;
        }

        /// <summary>
        /// 광고 동의 상태 문구를 설정한다.
        /// </summary>
        public void SetAdConsentStatus(string text)
        {
            if (_adConsentStatusText != null)
                _adConsentStatusText.text = text;
        }
    }

    /// <summary>
    /// 스토어 준수 패널 Presenter.
    /// </summary>
    public sealed class StoreCompliancePresenter : UIPresenter<StoreCompliancePanel>
    {
        public override void OnOpen()
        {
            RefreshContent();
            BindButtons();
            LocalizeTable.OnChangedLanguage += RefreshContent;
        }

        public override void OnClose()
        {
            LocalizeTable.OnChangedLanguage -= RefreshContent;
        }

        private void BindButtons()
        {
            BindButton(View.PrivacyButton, StoreComplianceManager.OpenPrivacyPolicy);
            BindButton(View.TermsButton, StoreComplianceManager.OpenTermsOfService);
            BindButton(View.AccountDeleteButton, StoreComplianceManager.OpenAccountDeletionRequest);
            BindButton(View.GachaRateButton, OpenGachaRateDisclosure);
            BindButton(View.AdConsentButton, RequestAdConsent);
            BindButton(View.CloseButton, () => UIManager.Close(View));
        }

        private void BindButton(CommonButton button, System.Action handler)
        {
            if (button == null || handler == null)
                return;

            button.OnClickAsObservable()
                .Subscribe(_ => handler())
                .AddTo(View);
        }

        private void RefreshContent()
        {
            View.SetTitle(StoreComplianceCopy.PanelTitle);
            View.SetTeenPaymentNotice(StoreComplianceCopy.TeenPaymentNotice);
            View.SetNotChildrenNotice(StoreComplianceCopy.NotTargetingChildren);
            RefreshAdConsentStatus();

            SetButtonLabel(View.PrivacyButton, StoreComplianceCopy.PrivacyPolicy);
            SetButtonLabel(View.TermsButton, StoreComplianceCopy.TermsOfService);
            SetButtonLabel(View.AccountDeleteButton, StoreComplianceCopy.AccountDeletion);
            SetButtonLabel(View.GachaRateButton, StoreComplianceCopy.GachaRateInfo);
            SetButtonLabel(View.AdConsentButton, StoreComplianceCopy.AdConsentManage);
            SetButtonLabel(View.CloseButton, StoreComplianceCopy.Close);
        }

        private void RefreshAdConsentStatus()
        {
            if (!StoreComplianceManager.IsAdConsentResolved())
            {
                View.SetAdConsentStatus(string.Empty);
                return;
            }

            var statusText = StoreComplianceManager.CanRequestPersonalizedAds()
                ? StoreComplianceCopy.AdConsentGranted
                : StoreComplianceCopy.AdConsentDenied;
            View.SetAdConsentStatus(statusText);
        }

        private void OpenGachaRateDisclosure()
        {
            UIManager.OpenAsync<GachaRateDisclosurePopup>().Forget();
        }

        private void RequestAdConsent()
        {
            RequestAdConsentAsync().Forget();
        }

        private async UniTaskVoid RequestAdConsentAsync()
        {
            await StoreComplianceManager.RequestAdConsentAsync();
            RefreshAdConsentStatus();
        }

        private static void SetButtonLabel(CommonButton button, string text)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = text;
        }
    }
}
