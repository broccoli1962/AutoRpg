using Backend.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭 '길드시설' — 시설 레벨 요약 및 업그레이드 (12_UIUX.md).
    /// </summary>
    public sealed class GuildFacilityRuntimePanel : ExplorationOverlayView
    {
        [SerializeField] private Text _contentText;
        private System.Action _onChanged;

        /// <summary>변경 콜백을 등록한다.</summary>
        public void Configure(System.Action onChanged)
        {
            _onChanged = onChanged;
        }

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!IsVisible)
                return;

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha6, KeyCode.Keypad6))
            {
                ScriptoriumManager.TryUpgrade(out var message);
                Debug.Log($"[GuildFacilityRuntimePanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha7, KeyCode.Keypad7))
            {
                TrainingGroundManager.TryUpgrade(out var message);
                Debug.Log($"[GuildFacilityRuntimePanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha8, KeyCode.Keypad8))
            {
                BlacksmithManager.TryUpgrade(out var message);
                Debug.Log($"[GuildFacilityRuntimePanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha9, KeyCode.Keypad9))
            {
                InnManager.TryUpgrade(out var message);
                Debug.Log($"[GuildFacilityRuntimePanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha0, KeyCode.Keypad0))
            {
                BookshopManager.TryUpgrade(out var message);
                Debug.Log($"[GuildFacilityRuntimePanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Minus, KeyCode.KeypadMinus))
            {
                SkillTreeManager.TryUpgradeLeaderRole(out var message);
                Debug.Log($"[GuildFacilityRuntimePanel] {message}");
                RefreshContent();
                _onChanged?.Invoke();
            }
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
                $"<b>6. 필사가의 서고</b>\n{ScriptoriumManager.GetDisplayLabel()}\n{ScriptoriumManager.GetBonusSummary()}\n\n" +
                $"<b>7. 훈련소</b>\n{TrainingGroundManager.GetDisplayLabel()}\n{TrainingGroundManager.GetBonusSummary()}\n\n" +
                $"<b>8. 대장간</b>\n{BlacksmithManager.GetDisplayLabel()}\n{BlacksmithManager.GetBonusSummary()}\n\n" +
                $"<b>9. 여관</b>\n{InnManager.GetDisplayLabel()}\n{InnManager.GetBonusSummary()}\n\n" +
                $"<b>0. 서점</b>\n{BookshopManager.GetDisplayLabel()}\n{BookshopManager.GetBonusSummary()}\n\n" +
                $"<b>-. 스킬 트리 (리더)</b>\n{SkillTreeManager.GetLeaderDisplayLabel()}";
        }
    }
}
