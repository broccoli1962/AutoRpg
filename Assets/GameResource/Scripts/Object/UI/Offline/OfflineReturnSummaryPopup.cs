using System.Text;
using Backend.GameSystems.Offline;
using Backend.Meta.Achievements;
using Backend.Object.Management;
using Backend.Util.Localization;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Offline
{
    /// <summary>
    /// 오프라인 복귀 요약 모달 View.
    /// </summary>
    public sealed class OfflineReturnSummaryPopup : UIPopup<OfflineReturnSummaryPresenter>
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _elapsedText;

        [Header("Sections")]
        [SerializeField] private Text _resourcesSectionTitle;
        [SerializeField] private Text _resourcesContent;
        [SerializeField] private Text _highlightsSectionTitle;
        [SerializeField] private Text _highlightsContent;

        [Header("Actions")]
        [SerializeField] private CommonButton _confirmButton;

        public CommonButton ConfirmButton => _confirmButton;

        /// <summary>
        /// 제목을 설정한다.
        /// </summary>
        public void SetTitle(string text)
        {
            if (_titleText != null)
                _titleText.text = text;
        }

        /// <summary>
        /// 경과 시간 문구를 설정한다.
        /// </summary>
        public void SetElapsed(string text)
        {
            if (_elapsedText != null)
                _elapsedText.text = text;
        }

        /// <summary>
        /// 획득 자원 섹션을 설정한다.
        /// </summary>
        public void SetResourcesSection(string title, string content)
        {
            if (_resourcesSectionTitle != null)
                _resourcesSectionTitle.text = title;
            if (_resourcesContent != null)
                _resourcesContent.text = content;
        }

        /// <summary>
        /// 하이라이트 섹션을 설정한다.
        /// </summary>
        public void SetHighlightsSection(string title, string content)
        {
            if (_highlightsSectionTitle != null)
                _highlightsSectionTitle.text = title;
            if (_highlightsContent != null)
                _highlightsContent.text = content;
        }
    }

    /// <summary>
    /// 오프라인 복귀 요약 Presenter.
    /// </summary>
    public sealed class OfflineReturnSummaryPresenter : UIPresenter<OfflineReturnSummaryPopup>
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
            OfflineReturnSummaryContext.Clear();
        }

        private void BindButtons()
        {
            if (View.ConfirmButton == null)
                return;

            View.ConfirmButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    MetaGameplayEvents.ReportOfflineRewardClaimed();
                    UIManager.Close(View);
                })
                .AddTo(View);
        }

        private void RefreshContent()
        {
            var result = OfflineReturnSummaryContext.Pending;
            if (result == null)
            {
                View.SetTitle("offline.summary.title".GetLocalizeText());
                return;
            }

            View.SetTitle("offline.summary.title".GetLocalizeText());
            View.SetElapsed("offline.summary.elapsed".GetLocalizeText(
                LocaleFormatUtil.FormatDuration(result.SettledDuration)));
            View.SetResourcesSection(
                "offline.summary.resources".GetLocalizeText(),
                BuildResourcesContent(result));
            View.SetHighlightsSection(
                "offline.summary.highlights".GetLocalizeText(),
                BuildHighlightsContent(result));
            SetButtonLabel(View.ConfirmButton, "offline.summary.confirm".GetLocalizeText());
        }

        private static void SetButtonLabel(CommonButton button, string text)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = text;
        }

        private static string BuildResourcesContent(OfflineSettlementResult result)
        {
            if (result.GoldReward <= 0L)
                return string.Empty;

            return "offline.summary.gold_line".GetLocalizeText(
                LocaleFormatUtil.FormatCompactNumber(result.GoldReward));
        }

        private static string BuildHighlightsContent(OfflineSettlementResult result)
        {
            if (result.Highlights == null || result.Highlights.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var line in result.Highlights)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                builder.AppendLine(line);
            }

            return builder.ToString().TrimEnd();
        }
    }
}
