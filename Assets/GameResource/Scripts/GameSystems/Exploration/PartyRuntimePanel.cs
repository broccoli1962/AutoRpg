using System.Text;
using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Equipment;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 프로토타입 파티/캐릭터 카드 좌측 패널 (12_UIUX.md).
    /// </summary>
    public sealed class PartyRuntimePanel : MonoBehaviour
    {
        private const float PanelWidth = 280f;

        private Text _contentText;
        private CompositeDisposable _disposables;
        private readonly StringBuilder _builder = new();

        public static float PanelWidthPx => PanelWidth;

        private void Start()
        {
            BuildUi();
            RelationshipManager.EnsureInitialized();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            Refresh(ExplorationManager.GetCurrentState());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void BuildUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            var root = new GameObject("PartyPanel");
            root.transform.SetParent(canvas.transform, false);

            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(
                ExplorationHudLayoutMetrics.HorizontalPadding,
                -ExplorationHudLayoutMetrics.TopBarHeight);
            rect.sizeDelta = new Vector2(
                PanelWidth,
                Screen.height - ExplorationHudLayoutMetrics.TopBarHeight - ExplorationHudLayoutMetrics.BottomInsetPx);

            var image = root.AddComponent<Image>();
            image.color = new Color(0.08f, 0.08f, 0.12f, 0.88f);

            var title = CreateText(root.transform, "PartyTitle", new Vector2(12f, -8f), 18, "[ 파티 ]");
            title.rectTransform.sizeDelta = new Vector2(PanelWidth - 24f, 28f);

            _contentText = CreateText(root.transform, "PartyContent", new Vector2(12f, -40f), 14, string.Empty);
            _contentText.rectTransform.sizeDelta = new Vector2(PanelWidth - 24f, Screen.height - 200f);
            _contentText.lineSpacing = 1.1f;
        }

        private void Refresh(ExplorationState state)
        {
            if (_contentText == null)
                return;

            _builder.Clear();

            var members = state?.Party?.Members;
            if (members == null || members.Count == 0)
            {
                _contentText.text = "파티 정보 없음";
                return;
            }

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0)
                    _builder.AppendLine();

                AppendMemberCard(members[i], i == 0);
            }

            var relationshipSummary = RelationshipManager.BuildHudSummary(state?.Party);
            if (!string.IsNullOrEmpty(relationshipSummary))
            {
                _builder.AppendLine();
                _builder.Append(relationshipSummary);
            }

            _contentText.text = _builder.ToString();
        }

        private void AppendMemberCard(CharacterState member, bool isLeader)
        {
            var hpRatio = member.MaxHp > 0 ? (float)member.CurrentHp / member.MaxHp : 0f;
            var hpColor = hpRatio > 0.5f ? "#9fd49f" : hpRatio > 0.25f ? "#e6c96f" : "#e07a7a";

            _builder.Append("<b>");
            if (isLeader)
                _builder.Append("★ ");

            _builder.Append(member.DisplayName);
            _builder.Append("</b> Lv.");
            _builder.Append(member.Level);
            _builder.Append(' ');
            _builder.Append(GetRoleLabel(member.Role));
            _builder.AppendLine("  <color=#8899aa>I:상세</color>");

            var tierTitle = CharacterTierManager.GetTierTitle(member.CharacterId);
            if (!string.IsNullOrEmpty(tierTitle) && tierTitle != "견습")
            {
                _builder.Append("<color=#c8b878>");
                _builder.Append(tierTitle);
                if (member.WeaponEnhanceLevel > 0 || member.ArmorEnhanceLevel > 0)
                {
                    _builder.Append(" · +");
                    _builder.Append(member.WeaponEnhanceLevel);
                    _builder.Append('/');
                    _builder.Append(member.ArmorEnhanceLevel);
                }

                _builder.AppendLine("</color>");
            }
            else if (member.WeaponEnhanceLevel > 0 || member.ArmorEnhanceLevel > 0)
            {
                _builder.Append("<color=#c8b878>강화 +");
                _builder.Append(member.WeaponEnhanceLevel);
                _builder.Append('/');
                _builder.Append(member.ArmorEnhanceLevel);
                _builder.AppendLine("</color>");
            }

            _builder.Append("<color=");
            _builder.Append(hpColor);
            _builder.Append(">HP ");
            _builder.Append(member.CurrentHp);
            _builder.Append('/');
            _builder.Append(member.MaxHp);
            _builder.Append("</color>");

            if (member.Injury != InjurySeverity.None)
            {
                _builder.Append(" · ");
                _builder.Append(GetInjuryLabel(member.Injury));
            }

            _builder.AppendLine();

            if (member.PersonalityTags != null && member.PersonalityTags.Count > 0)
            {
                _builder.Append("<color=#9ab0c8>");
                for (var i = 0; i < member.PersonalityTags.Count; i++)
                {
                    if (i > 0)
                        _builder.Append(", ");

                    _builder.Append(GetPersonalityLabel(member.PersonalityTags[i]));
                }

                _builder.AppendLine("</color>");
            }

            var equipment = EquipmentService.GetMemberEquipmentSummary(member);
            if (!string.IsNullOrEmpty(equipment))
            {
                _builder.Append("<color=#b8a878>");
                _builder.Append(equipment);
                _builder.AppendLine("</color>");
            }

            var memoryPreview = CharacterMemoryManager.BuildHudPreview(member.CharacterId);
            if (!string.IsNullOrEmpty(memoryPreview))
                _builder.AppendLine(memoryPreview);
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPos, int fontSize, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;

            var label = go.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.UpperLeft;
            label.color = Color.white;
            label.supportRichText = true;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private static string GetRoleLabel(CharacterRole role)
        {
            return role switch
            {
                CharacterRole.Warrior => "전사",
                CharacterRole.Rogue => "도적",
                CharacterRole.Mage => "마법사",
                CharacterRole.Bard => "음유시인",
                CharacterRole.Cleric => "성직자",
                _ => role.ToString()
            };
        }

        private static string GetPersonalityLabel(PersonalityTag tag)
        {
            return tag switch
            {
                PersonalityTag.Cautious => "신중",
                PersonalityTag.Greedy => "탐욕",
                PersonalityTag.Reckless => "무모",
                PersonalityTag.Cheerful => "쾌활",
                PersonalityTag.Loyal => "충직",
                PersonalityTag.Cynical => "냉소",
                _ => tag.ToString()
            };
        }

        private static string GetInjuryLabel(InjurySeverity severity)
        {
            return severity switch
            {
                InjurySeverity.Light => "경상",
                InjurySeverity.Moderate => "중상",
                InjurySeverity.Severe => "중증",
                InjurySeverity.Fatal => "치명",
                _ => severity.ToString()
            };
        }
    }
}
