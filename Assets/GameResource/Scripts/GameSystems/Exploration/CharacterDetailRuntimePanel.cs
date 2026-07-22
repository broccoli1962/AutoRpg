using System.Text;
using Backend.GameSystems.Character;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Exploration.Data;
using R3;
using UnityEngine;
using Backend.Util;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX 캐릭터 카드 클릭 대응 — 스탯·장비·스킬·관계·기억 상세 패널.
    /// </summary>
    public sealed class CharacterDetailRuntimePanel : ExplorationOverlayView
    {
        [SerializeField] private Text _contentText;
        [SerializeField] private Text _hintText;
        private int _memberIndex;
        private CompositeDisposable _disposables;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            CharacterMemoryManager.EnsureInitialized();
            _disposables = new CompositeDisposable();
            ExplorationChannels.OnStateChanged
                .Subscribe(_ => RefreshIfVisible())
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void Update()
        {
            if (IsOtherPanelVisible())
                return;

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.I))
            {
                if (IsVisible)
                    Hide();
                else
                    ShowMember(0);
            }

            if (!IsVisible)
                return;

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.Escape))
            {
                Hide();
                return;
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Q, KeyCode.Comma))
                CycleMember(-1);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.E, KeyCode.Period))
                CycleMember(1);
        }

        public void ShowMember(int index)
        {
            var members = ExplorationManager.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _memberIndex = Mathf.Clamp(index, 0, members.Count - 1);
            Show();
        }

        protected override void OnBeforeShow()
        {
            var members = ExplorationManager.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _memberIndex = Mathf.Clamp(_memberIndex, 0, members.Count - 1);
            RefreshContent(members[_memberIndex]);
        }

        private void RefreshIfVisible()
        {
            if (!IsVisible)
                return;

            var members = ExplorationManager.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
            {
                Hide();
                return;
            }

            _memberIndex = Mathf.Clamp(_memberIndex, 0, members.Count - 1);
            RefreshContent(members[_memberIndex]);
        }

        private void CycleMember(int delta)
        {
            var members = ExplorationManager.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _memberIndex = (_memberIndex + delta + members.Count) % members.Count;
            RefreshContent(members[_memberIndex]);
        }

        private void RefreshContent(CharacterState member)
        {
            if (_contentText == null || member == null)
                return;

            var party = ExplorationManager.GetCurrentState()?.Party;
            var members = party?.Members;
            var memberCount = members?.Count ?? 0;

            _contentText.text = BuildDetailText(member, party);
            _hintText.text = memberCount > 1
                ? $"I:닫기  Q/E:캐릭터 {_memberIndex + 1}/{memberCount}  Esc:닫기"
                : "I 또는 Esc:닫기";
        }

        private static string BuildDetailText(CharacterState member, PartyState party)
        {
            var builder = new StringBuilder();
            builder.Append("<b>[ ");
            builder.Append(member.DisplayName);
            builder.Append(" ]</b>  Lv.");
            builder.Append(member.Level);
            builder.Append(' ');
            builder.Append(GetRoleLabel(member.Role));
            builder.AppendLine();
            builder.AppendLine(CharacterTierManager.GetTierTitle(member.CharacterId));

            builder.AppendLine();
            builder.AppendLine("<b>스탯</b>");
            builder.Append("STR ");
            builder.Append(EquipmentService.GetEffectiveStr(member));
            builder.Append("  AGI ");
            builder.Append(EquipmentService.GetEffectiveAgi(member));
            builder.Append("  INT ");
            builder.Append(EquipmentService.GetEffectiveInt(member));
            builder.Append("  VIT ");
            builder.Append(EquipmentService.GetEffectiveVit(member));
            builder.Append("  LUK ");
            builder.Append(member.Luk);
            builder.AppendLine();
            builder.Append("HP ");
            builder.Append(member.CurrentHp);
            builder.Append('/');
            builder.Append(member.MaxHp);
            if (member.Injury != InjurySeverity.None)
            {
                builder.Append("  · ");
                builder.Append(GetInjuryLabel(member.Injury));
            }

            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("<b>장비</b>");
            builder.AppendLine(EquipmentService.GetMemberEquipmentSummary(member) ?? "장비 없음");
            builder.Append("강화 +");
            builder.Append(member.WeaponEnhanceLevel);
            builder.Append(" / +");
            builder.Append(member.ArmorEnhanceLevel);

            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("<b>스킬</b>");
            builder.AppendLine(SkillTreeManager.GetDisplayLabel(member.Role));

            builder.AppendLine();
            builder.AppendLine("<b>관계</b>");
            builder.AppendLine(BuildRelationshipLines(member, party));

            var memoryPreview = CharacterMemoryManager.BuildHudPreview(member.CharacterId);
            builder.AppendLine();
            builder.AppendLine("<b>최근 기억</b>");
            builder.AppendLine(string.IsNullOrEmpty(memoryPreview) ? "기록 없음" : memoryPreview);

            return builder.ToString();
        }

        private static string BuildRelationshipLines(CharacterState member, PartyState party)
        {
            if (party?.Members == null || party.Members.Count < 2)
                return "다른 파티원 없음";

            var builder = new StringBuilder();
            foreach (var other in party.Members)
            {
                if (other.CharacterId == member.CharacterId)
                    continue;

                var affinity = RelationshipManager.GetAffinity(member.CharacterId, other.CharacterId);
                var bond = affinity >= 60 ? " ★본드" : string.Empty;
                builder.Append(other.DisplayName);
                builder.Append(": ");
                builder.Append(affinity);
                builder.Append(bond);
                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private bool IsOtherPanelVisible()
        {
            if (GetComponent<ChronicleRuntimePanel>()?.IsVisible == true)
                return true;

            if (GetComponent<ExplorationSettingsRuntimePanel>()?.IsVisible == true)
                return true;

            if (GetComponent<EnhanceRuntimePanel>()?.IsVisible == true)
                return true;

            return GetComponent<GuildFacilityRuntimePanel>()?.IsVisible == true;
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
    }
}
