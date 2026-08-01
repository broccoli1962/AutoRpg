using Backend.Meta.Achievements;
using Backend.Object.Management;
using Backend.Object.UI.Gacha;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

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

        public override UILayer Layer => UILayer.HUD;

        protected override void OnOpen()
        {
            BindTouchActions();
        }

        private void BindTouchActions()
        {
            BindButton(_dispatchButton, OnDispatchTapped);
            BindButton(_returnButton, OnReturnTapped);
            BindButton(_enhanceButton, OnEnhanceTapped);
            BindButton(_summonButton, OnSummonTapped);
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
    }
}
