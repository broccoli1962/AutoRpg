using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 좌측 파티 멤버 카드 1장. 프리팹에서 4개 고정 배치.
    /// </summary>
    public sealed class PartyMemberCardView : CachedMonobehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private TextMeshProUGUI _detailText;
        [SerializeField] private Image _portraitFrame;
        [SerializeField] private Image _portraitTint;
        [SerializeField] private Image _hpTrack;
        [SerializeField] private Image _hpFill;

        /// <summary>캐릭터 상태를 카드에 바인딩한다.</summary>
        public void Bind(CharacterState member, bool isLeader)
        {
            if (member == null)
            {
                CachedGameObject.SetActive(false);
                return;
            }

            CachedGameObject.SetActive(true);

            if (_nameText != null)
                _nameText.text = isLeader ? $"★ {member.DisplayName}" : member.DisplayName;

            if (_roleText != null)
                _roleText.text = ExplorationHudStatusFormatter.BuildCardRoleLine(member);

            if (_portraitTint != null)
                _portraitTint.color = Color.Lerp(ExplorationHudStatusFormatter.GetRoleTintColor(member.Role), Color.white, 0.25f);

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
                _detailText.text = ExplorationHudStatusFormatter.BuildCardDetailLine(member);

            ApplyTextLayout();
            UiTmpUtil.RebuildLogItemLayout(transform as RectTransform, _detailText);
        }

        private void ApplyTextLayout()
        {
            // 글자 크기는 Prefab. 색·말줄임만 런타임 보정.
            if (_nameText != null)
            {
                _nameText.overflowMode = TextOverflowModes.Ellipsis;
                _nameText.textWrappingMode = TextWrappingModes.NoWrap;
            }

            if (_roleText != null)
            {
                _roleText.color = ModernUiStyle.MutedText;
                _roleText.overflowMode = TextOverflowModes.Ellipsis;
                _roleText.textWrappingMode = TextWrappingModes.NoWrap;
            }

            if (_detailText != null)
            {
                _detailText.color = ModernUiStyle.MutedText;
                _detailText.overflowMode = TextOverflowModes.Ellipsis;
                _detailText.textWrappingMode = TextWrappingModes.Normal;
            }
        }
    }
}
