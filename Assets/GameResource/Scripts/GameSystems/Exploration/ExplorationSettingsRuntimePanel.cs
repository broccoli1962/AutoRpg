using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Stage;
using Backend.GameSystems.LLM;
using Backend.Object.UI;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 탐험 설정 런타임 패널 (12_UIUX.md LLM/이벤트 옵션).
    /// </summary>
    public sealed class ExplorationSettingsRuntimePanel : ExplorationOverlayView<ExplorationSettingsRuntimePresenter>
    {
        [SerializeField] private TextMeshProUGUI _contentText;

        public void Configure(System.Action onSettingsChanged)
        {
            EnsurePresenterReady();
            Presenter.Configure(onSettingsChanged);
        }

        public void HandleInput() => ReadyPresenter.HandleKeyboardInput();

        public void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        internal TextMeshProUGUI ContentText => _contentText;
    }

    public sealed class ExplorationSettingsRuntimePresenter : UIPresenter<ExplorationSettingsRuntimePanel>
    {
        private System.Action _onSettingsChanged;

        public void Configure(System.Action onSettingsChanged) => _onSettingsChanged = onSettingsChanged;

        public override void OnOpen() => RefreshContent();

        public void HandleKeyboardInput()
        {
            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
            {
                LlmQualitySettings.CycleMode();
                RefreshAndNotify();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
            {
                DynamicEventAutoPolicySettings.CyclePolicy();
                RefreshAndNotify();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
            {
                GoldenEventSettings.ToggleAutoPause();
                RefreshAndNotify();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha4, KeyCode.Keypad4))
            {
                LogFrequencySettings.CycleMode();
                RefreshAndNotify();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha5, KeyCode.Keypad5))
            {
                OfflineSummaryDetailSettings.ToggleMode();
                RefreshAndNotify();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha0, KeyCode.Keypad0))
            {
                StageVfxDensitySettings.CycleMode();
                RefreshAndNotify();
            }

            if (TryFacilityUpgrade(KeyCode.Alpha6, KeyCode.Keypad6, ScriptoriumSystem.TryUpgrade))
                return;

            if (TryFacilityUpgrade(KeyCode.Alpha7, KeyCode.Keypad7, TrainingGroundSystem.TryUpgrade))
                return;

            if (TryFacilityUpgrade(KeyCode.Alpha8, KeyCode.Keypad8, BlacksmithSystem.TryUpgrade))
                return;

            if (TryFacilityUpgrade(KeyCode.Alpha9, KeyCode.Keypad9, InnSystem.TryUpgrade))
                return;

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Minus, KeyCode.KeypadMinus))
                TryFacilityUpgrade(SkillTreeSystem.TryUpgradeLeaderRole);
        }

        private bool TryFacilityUpgrade(KeyCode primary, KeyCode keypad, TryActionDelegate action)
        {
            if (!KeyboardInputUtil.WasAnyKeyPressedThisFrame(primary, keypad))
                return false;

            TryFacilityUpgrade(action);
            return true;
        }

        private void TryFacilityUpgrade(TryActionDelegate action)
        {
            var success = action(out var message);
            Debug.Log($"[ExplorationSettingsRuntimePanel] {(success ? message : $"업그레이드 불가: {message}")}");
            RefreshAndNotify();
        }

        private void RefreshAndNotify()
        {
            RefreshContent();
            _onSettingsChanged?.Invoke();
        }

        private void RefreshContent()
        {
            if (View.ContentText == null)
                return;

            View.ContentText.text =
                $"<b>1. LLM 텍스트 품질</b>\n{LlmQualitySettings.GetDisplayLabel()}\n\n" +
                $"<b>2. 자동 이벤트 선택 정책</b>\n{DynamicEventAutoPolicySettings.GetDisplayLabel()}\n\n" +
                $"<b>3. 황금 이벤트 자동정지</b>\n{GoldenEventSettings.GetDisplayLabel()}\n\n" +
                $"<b>4. 로그 생성 빈도</b>\n{LogFrequencySettings.GetDisplayLabel()} · 최소 {GetSalienceLabel(LogFrequencySettings.GetMinimumSalience())}\n\n" +
                $"<b>5. 오프라인 요약 상세도</b>\n{OfflineSummaryDetailSettings.GetDisplayLabel()}\n\n" +
                $"<b>0. 스테이지 연출 밀도</b>\n{StageVfxDensitySettings.GetDisplayLabel()}\n\n" +
                ExplorationHudStatusFormatter.BuildSettingsFacilityBlock() +
                $"\n\n<b>설정 요약</b>\n{ExplorationHudStatusFormatter.BuildSettingsSummary()}";
        }

        private static string GetSalienceLabel(SalienceGrade grade) =>
            grade switch
            {
                SalienceGrade.Significant => "Significant+",
                SalienceGrade.Trivial => "전체",
                _ => "Notable+"
            };

        private delegate bool TryActionDelegate(out string message);
    }
}
