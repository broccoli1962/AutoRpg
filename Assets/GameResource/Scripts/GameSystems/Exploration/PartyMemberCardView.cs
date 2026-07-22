using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Equipment;
using Backend.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 좌측 파티 멤버 카드 1장. 프리팹에서 4개 고정 배치.
    /// </summary>
    public sealed class PartyMemberCardView : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _roleText;
        [SerializeField] private Text _detailText;
        [SerializeField] private Image _portraitFrame;
        [SerializeField] private Image _portraitTint;
        [SerializeField] private Image _hpTrack;
        [SerializeField] private Image _hpFill;

        public void Bind(CharacterState member, bool isLeader)
        {
            if (member == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_nameText != null)
            {
                _nameText.text = isLeader ? $"★ {member.DisplayName}" : member.DisplayName;
                ModernUiStyle.ApplyTitleMedium(_nameText);
            }

            if (_roleText != null)
            {
                _roleText.text = $"Lv.{member.Level} {GetRoleLabel(member.Role)}";
                ModernUiStyle.ApplyMuted(_roleText, 12);
            }

            if (_portraitTint != null)
                _portraitTint.color = Color.Lerp(GetRoleTintColor(member.Role), Color.white, 0.25f);

            if (_portraitFrame != null)
                RuntimeUiSprites.ApplyPortraitFrame(_portraitFrame);

            var hpRatio = member.MaxHp > 0 ? (float)member.CurrentHp / member.MaxHp : 0f;
            if (_hpFill != null)
            {
                _hpFill.type = Image.Type.Filled;
                _hpFill.fillMethod = Image.FillMethod.Horizontal;
                _hpFill.color = hpRatio > 0.25f ? ModernUiStyle.AccentGreen : ModernUiStyle.DangerRed;
                _hpFill.fillAmount = hpRatio;
            }

            if (_detailText != null)
                _detailText.text = BuildDetailLine(member);
        }

        private static string BuildDetailLine(CharacterState member)
        {
            var equipment = EquipmentService.GetMemberEquipmentSummary(member);
            var injury = member.Injury != InjurySeverity.None ? GetInjuryLabel(member.Injury) : string.Empty;
            if (!string.IsNullOrEmpty(equipment) && !string.IsNullOrEmpty(injury))
                return $"{equipment} · {injury}";

            return !string.IsNullOrEmpty(equipment) ? equipment : injury;
        }

        private static string GetRoleLabel(CharacterRole role) =>
            role switch
            {
                CharacterRole.Warrior => "전사",
                CharacterRole.Rogue => "도적",
                CharacterRole.Mage => "마법사",
                CharacterRole.Bard => "음유시인",
                CharacterRole.Cleric => "성직자",
                _ => role.ToString()
            };

        private static string GetInjuryLabel(InjurySeverity severity) =>
            severity switch
            {
                InjurySeverity.Light => "경상",
                InjurySeverity.Moderate => "중상",
                InjurySeverity.Severe => "중증",
                InjurySeverity.Fatal => "치명",
                _ => severity.ToString()
            };

        private static Color GetRoleTintColor(CharacterRole role) =>
            role switch
            {
                CharacterRole.Warrior => new Color(0.88f, 0.48f, 0.48f, 1f),
                CharacterRole.Rogue => new Color(0.62f, 0.83f, 0.62f, 1f),
                CharacterRole.Mage => new Color(0.43f, 0.77f, 1f, 1f),
                CharacterRole.Bard => new Color(1f, 0.85f, 0.4f, 1f),
                CharacterRole.Cleric => new Color(0.79f, 0.63f, 1f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f, 1f)
            };
    }
}
