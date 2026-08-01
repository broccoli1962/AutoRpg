using Backend.Meta.Gacha;
using Backend.Object.Management;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Gacha
{
    /// <summary>
    /// 소환 메인 패널 View. 확률 정보 진입 링크와 소환 버튼을 제공한다.
    /// </summary>
    public sealed class GachaSummonPanel : UIPanel<GachaSummonPresenter>
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;

        [Header("Actions")]
        [SerializeField] private CommonButton _rateInfoButton;
        [SerializeField] private CommonButton _singleSummonButton;
        [SerializeField] private CommonButton _tenSummonButton;
        [SerializeField] private CommonButton _closeButton;

        [Header("Rate Info Link")]
        [SerializeField] private Text _rateInfoLabel;

        public CommonButton RateInfoButton => _rateInfoButton;
        public CommonButton SingleSummonButton => _singleSummonButton;
        public CommonButton TenSummonButton => _tenSummonButton;
        public CommonButton CloseButton => _closeButton;

        /// <summary>
        /// 제목 텍스트를 설정한다.
        /// </summary>
        public void SetTitle(string text)
        {
            if (_titleText != null)
                _titleText.text = text;
        }

        /// <summary>
        /// 확률 정보 링크 라벨을 설정한다.
        /// </summary>
        public void SetRateInfoLabel(string text)
        {
            if (_rateInfoLabel != null)
                _rateInfoLabel.text = text;
        }

        /// <summary>
        /// 단차 버튼 라벨을 설정한다.
        /// </summary>
        public void SetSingleSummonLabel(string text)
        {
            SetButtonLabel(_singleSummonButton, text);
        }

        /// <summary>
        /// 10연차 버튼 라벨을 설정한다.
        /// </summary>
        public void SetTenSummonLabel(string text)
        {
            SetButtonLabel(_tenSummonButton, text);
        }

        private static void SetButtonLabel(CommonButton button, string text)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<Text>();
            if (label == null)
                return;

            label.text = text;
        }
    }

    /// <summary>
    /// 소환 패널 Presenter. 확률 공시 팝업 진입과 소환 액션을 처리한다.
    /// </summary>
    public sealed class GachaSummonPresenter : UIPresenter<GachaSummonPanel>
    {
        public override void OnOpen()
        {
            RefreshLabels();
            BindButtons();
            LocalizeTable.OnChangedLanguage += RefreshLabels;
        }

        public override void OnClose()
        {
            LocalizeTable.OnChangedLanguage -= RefreshLabels;
        }

        private void BindButtons()
        {
            Bind(View.RateInfoButton, OpenRateDisclosureAsync);
            Bind(View.CloseButton, CloseSelf);
        }

        private void Bind(CommonButton button, System.Action handler)
        {
            if (button == null || handler == null)
                return;

            button.OnClickAsObservable()
                .Subscribe(_ => handler())
                .AddTo(View);
        }

        private void OpenRateDisclosureAsync()
        {
            UIManager.OpenAsync<GachaRateDisclosurePopup>().Forget();
        }

        private void CloseSelf()
        {
            UIManager.Close(View);
        }

        private void RefreshLabels()
        {
            View.SetTitle("gacha.summon.title".GetLocalizeText());
            View.SetRateInfoLabel("gacha.summon.rate_info".GetLocalizeText());
            View.SetSingleSummonLabel("gacha.summon.single".GetLocalizeText());
            View.SetTenSummonLabel("gacha.summon.ten".GetLocalizeText());
            SetButtonLabel(View.CloseButton, "ui.common.close".GetLocalizeText());
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
