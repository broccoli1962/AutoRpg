using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.LLM;
using UnityEngine;
using Backend.Util;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 탐험 설정 런타임 패널 (12_UIUX.md LLM/이벤트 옵션).
    /// </summary>
    public sealed class ExplorationSettingsRuntimePanel : ExplorationOverlayView
    {
        [SerializeField] private Text _contentText;
        private System.Action _onSettingsChanged;

        public void Configure(System.Action onSettingsChanged)
        {
            _onSettingsChanged = onSettingsChanged;
        }

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!IsVisible)
                return;

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
            {
                LlmQualitySettings.CycleMode();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
            {
                DynamicEventAutoPolicySettings.CyclePolicy();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
            {
                GoldenEventSettings.ToggleAutoPause();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha4, KeyCode.Keypad4))
            {
                LogFrequencySettings.CycleMode();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha5, KeyCode.Keypad5))
            {
                OfflineSummaryDetailSettings.ToggleMode();
                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha6, KeyCode.Keypad6))
            {
                if (ScriptoriumManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettingsRuntimePanel] {message}");
                else
                    Debug.Log($"[ExplorationSettingsRuntimePanel] 서고 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha7, KeyCode.Keypad7))
            {
                if (TrainingGroundManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettingsRuntimePanel] {message}");
                else
                    Debug.Log($"[ExplorationSettingsRuntimePanel] 훈련소 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha8, KeyCode.Keypad8))
            {
                if (BlacksmithManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettingsRuntimePanel] {message}");
                else
                    Debug.Log($"[ExplorationSettingsRuntimePanel] 대장간 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha9, KeyCode.Keypad9))
            {
                if (InnManager.TryUpgrade(out var message))
                    Debug.Log($"[ExplorationSettingsRuntimePanel] {message}");
                else
                    Debug.Log($"[ExplorationSettingsRuntimePanel] 여관 업그레이드 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Minus, KeyCode.KeypadMinus))
            {
                if (SkillTreeManager.TryUpgradeLeaderRole(out var message))
                    Debug.Log($"[ExplorationSettingsRuntimePanel] {message}");
                else
                    Debug.Log($"[ExplorationSettingsRuntimePanel] 스킬 해금 불가: {message}");

                RefreshContent();
                _onSettingsChanged?.Invoke();
            }
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        protected override void OnBeforeShow()
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            if (_contentText == null)
                return;

            _contentText.text =
                $"<b>1. LLM 텍스트 품질</b>\n{LlmQualitySettings.GetDisplayLabel()}\n\n" +
                $"<b>2. 자동 이벤트 선택 정책</b>\n{DynamicEventAutoPolicySettings.GetDisplayLabel()}\n\n" +
                $"<b>3. 황금 이벤트 자동정지</b>\n{GoldenEventSettings.GetDisplayLabel()}\n\n" +
                $"<b>4. 로그 생성 빈도</b>\n{LogFrequencySettings.GetDisplayLabel()} · 최소 {GetSalienceLabel(LogFrequencySettings.GetMinimumSalience())}\n\n" +
                $"<b>5. 오프라인 요약 상세도</b>\n{OfflineSummaryDetailSettings.GetDisplayLabel()}\n\n" +
                $"<b>6. 필사가의 서고</b>\n{ScriptoriumManager.GetDisplayLabel()}\n{ScriptoriumManager.GetBonusSummary()}\n\n" +
                $"<b>7. 훈련소</b>\n{TrainingGroundManager.GetDisplayLabel()}\n{TrainingGroundManager.GetBonusSummary()}\n\n" +
                $"<b>8. 대장간</b>\n{BlacksmithManager.GetDisplayLabel()}\n{BlacksmithManager.GetBonusSummary()}\n\n" +
                $"<b>9. 여관</b>\n{InnManager.GetDisplayLabel()}\n{InnManager.GetBonusSummary()}\n\n" +
                $"<b>0. 서점</b>\n{BookshopManager.GetDisplayLabel()}\n{BookshopManager.GetBonusSummary()}\n\n" +
                $"<b>-. 스킬 트리 (리더)</b>\n{SkillTreeManager.GetLeaderDisplayLabel()}\n\n" +
                $"<b>설정 요약</b>\n{ExplorationHudStatusFormatter.BuildSettingsSummary()}";
        }

        private static string GetSalienceLabel(SalienceGrade grade)
        {
            return grade switch
            {
                SalienceGrade.Significant => "Significant+",
                SalienceGrade.Trivial => "전체",
                _ => "Notable+"
            };
        }
    }
}
