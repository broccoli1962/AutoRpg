using System.Text;
using Backend.Meta;
using Backend.Meta.Gacha;
using Backend.Object.Management;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Gacha
{
    /// <summary>
    /// 게임산업법 확률형 아이템 정보공개 팝업 View.
    /// </summary>
    public sealed class GachaRateDisclosurePopup : UIPopup<GachaRateDisclosurePresenter>
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;

        [Header("Sections")]
        [SerializeField] private Text _gradeSectionTitle;
        [SerializeField] private Text _gradeContent;
        [SerializeField] private Text _itemSectionTitle;
        [SerializeField] private Text _itemContent;
        [SerializeField] private Text _pitySectionTitle;
        [SerializeField] private Text _pityContent;
        [SerializeField] private Text _tenPullContent;

        [Header("Actions")]
        [SerializeField] private CommonButton _closeButton;

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
        /// 등급별 확률 섹션을 설정한다.
        /// </summary>
        public void SetGradeSection(string title, string content)
        {
            if (_gradeSectionTitle != null)
                _gradeSectionTitle.text = title;
            if (_gradeContent != null)
                _gradeContent.text = content;
        }

        /// <summary>
        /// 개별 항목 확률 섹션을 설정한다.
        /// </summary>
        public void SetItemSection(string title, string content)
        {
            if (_itemSectionTitle != null)
                _itemSectionTitle.text = title;
            if (_itemContent != null)
                _itemContent.text = content;
        }

        /// <summary>
        /// 천장 섹션을 설정한다.
        /// </summary>
        public void SetPitySection(string title, string content)
        {
            if (_pitySectionTitle != null)
                _pitySectionTitle.text = title;
            if (_pityContent != null)
                _pityContent.text = content;
        }

        /// <summary>
        /// 10연차 보장 문구를 설정한다.
        /// </summary>
        public void SetTenPullGuarantee(string content)
        {
            if (_tenPullContent != null)
                _tenPullContent.text = content;
        }
    }

    /// <summary>
    /// 확률 공시 팝업 Presenter. GachaRateTable 단일 출처로 표시 내용을 구성한다.
    /// </summary>
    public sealed class GachaRateDisclosurePresenter : UIPresenter<GachaRateDisclosurePopup>
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
            if (View.CloseButton == null)
                return;

            View.CloseButton.OnClickAsObservable()
                .Subscribe(_ => UIManager.Close(View))
                .AddTo(View);
        }

        private void RefreshContent()
        {
            var rateTable = GachaDataProvider.GetRateTable();
            var bannerPool = GachaDataProvider.GetBannerPool();
            var pity = MetaRuntimeProvider.Gacha.Pity;
            var snapshot = GachaRateDisclosureBuilder.Build(rateTable, bannerPool, pity);

            View.SetTitle("gacha.rate.title".GetLocalizeText());
            View.SetGradeSection(
                "gacha.rate.grade_section".GetLocalizeText(),
                BuildGradeContent(snapshot));
            View.SetItemSection(
                "gacha.rate.item_section".GetLocalizeText(),
                BuildItemContent(snapshot));
            View.SetPitySection(
                "gacha.rate.pity_section".GetLocalizeText(),
                BuildPityContent(snapshot));
            View.SetTenPullGuarantee("gacha.rate.ten_pull".GetLocalizeText());
        }

        private static string BuildGradeContent(GachaRateDisclosureSnapshot snapshot)
        {
            var builder = new StringBuilder();

            foreach (var entry in snapshot.GradeRates)
            {
                var gradeName = entry.GradeLocalizeKey.GetLocalizeText();
                var percent = GachaRateDisclosureBuilder.FormatPercent(entry.RateBasisPoints);
                builder.AppendLine("gacha.rate.grade_line".GetLocalizeText(gradeName, percent));
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildItemContent(GachaRateDisclosureSnapshot snapshot)
        {
            var builder = new StringBuilder();

            foreach (var entry in snapshot.ItemRates)
            {
                var name = entry.CharacterNameLocalizeKey.GetLocalizeText();
                var percent = GachaRateDisclosureBuilder.FormatPercent(entry.RateBasisPoints);
                builder.AppendLine("gacha.rate.item_line".GetLocalizeText(name, percent));
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildPityContent(GachaRateDisclosureSnapshot snapshot)
        {
            var builder = new StringBuilder();
            builder.AppendLine("gacha.rate.ssr_pity".GetLocalizeText(
                snapshot.SsrPityCounter,
                snapshot.SsrPityThreshold));
            builder.Append("gacha.rate.ur_pity".GetLocalizeText(
                snapshot.UrPityCounter,
                snapshot.UrPityThreshold));
            return builder.ToString();
        }
    }
}
