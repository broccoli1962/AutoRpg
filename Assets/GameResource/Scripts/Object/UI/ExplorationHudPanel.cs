using Backend.Meta.Achievements;
using Backend.Object.Management;
using Backend.Object.UI.Gacha;
using Backend.Object.UI.StoreCompliance;
using Backend.Util.Localization;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI
{
    /// <summary>
    /// 인게임 탐험 HUD. 파견·귀환 등 주요 액션을 터치 버튼으로 제공한다.
    /// </summary>
    public sealed class ExplorationHudPanel : UIPanel
    {
        [Header("Touch Actions")]
        [SerializeField] private CommonButton _dispatchButton;
        [SerializeField] private CommonButton _returnButton;
        [SerializeField] private CommonButton _enhanceButton;
        [SerializeField] private CommonButton _summonButton;
        [SerializeField] private CommonButton _settingsButton;

        public override UILayer Layer => UILayer.HUD;

        protected override void OnOpen()
        {
            RefreshLabels();
            BindTouchActions();
            LocalizeTable.OnChangedLanguage += RefreshLabels;
        }

        protected override void OnClose()
        {
            LocalizeTable.OnChangedLanguage -= RefreshLabels;
            base.OnClose();
        }

        private void RefreshLabels()
        {
            SetButtonLabel(_dispatchButton, "ui.hud.dispatch".GetLocalizeText());
            SetButtonLabel(_enhanceButton, "ui.hud.enhance".GetLocalizeText());
            SetButtonLabel(_summonButton, "ui.hud.summon".GetLocalizeText());
            SetButtonLabel(_returnButton, "ui.hud.return".GetLocalizeText());
            SetButtonLabel(_settingsButton, ResolveSettingsLabel());
        }

        private static void SetButtonLabel(CommonButton button, string text)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = text;
        }

        private void BindTouchActions()
        {
            BindButton(_dispatchButton, OnDispatchTapped);
            BindButton(_returnButton, OnReturnTapped);
            BindButton(_enhanceButton, OnEnhanceTapped);
            BindButton(_summonButton, OnSummonTapped);
            BindButton(_settingsButton, OnSettingsTapped);
        }

        private static string ResolveSettingsLabel()
        {
            var localized = "ui.hud.settings".GetLocalizeText();
            if (!string.IsNullOrEmpty(localized) && localized != "ui.hud.settings" && !localized.StartsWith("!"))
                return localized;

            return "설정";
        }

        private void BindButton(CommonButton button, System.Action handler)
        {
            if (button == null || handler == null)
                return;

            button.OnClickAsObservable()
                .Subscribe(_ => handler())
                .AddTo(this);
        }

        private void OnDispatchTapped()
        {
            MetaGameplayEvents.ReportDispatchStarted();
        }

        private void OnReturnTapped()
        {
        }

        private void OnEnhanceTapped()
        {
        }

        private void OnSummonTapped()
        {
            UIManager.OpenAsync<GachaSummonPanel>().Forget();
        }

        private void OnSettingsTapped()
        {
            UIManager.OpenAsync<StoreCompliancePanel>().Forget();
        }
    }
}
