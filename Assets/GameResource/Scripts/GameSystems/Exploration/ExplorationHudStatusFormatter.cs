using System.Text;
using Backend.GameSystems.Character;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Equipment.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Stage;
using Backend.GameSystems.LLM;
using Backend.GameSystems.Prestige;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Exploration HUD 문자열·캐릭터 표시·패널 본문 포맷.
    /// </summary>
    public static class ExplorationHudStatusFormatter
    {
        public const string GuildDisplayName = "등불 수호단";

        public static string Build(ExplorationState state)
        {
            if (state == null)
                return "탐험 상태 없음";

            return $"{BuildTopResourceBar(state)}\n{BuildExplorationLine(state)}";
        }

        public static string BuildTopResourceBar(ExplorationState state)
        {
            var meta = PrestigeManager.GetMeta();
            var gold = state.Gold;
            var mana = state.ManaShards + (meta?.ManaShards ?? 0);
            var reputation = state.Reputation + (meta?.Reputation ?? 0);
            var relic = state.RelicFragments + (meta?.RelicFragments ?? 0);
            var legacy = meta?.LegacyPoints ?? 0;

            return
                $"<b>{GuildDisplayName}</b>  ·  <color=#e8c547>G {gold}</color>  ·  <color=#6ec5ff>◆ {mana}</color>\n" +
                $"<color=#c9a0ff>★ {reputation}</color>  ·  <color=#ffb366>◈ {relic}</color>  ·  <color=#9fd89f>유산 {legacy}</color>";
        }

        public static string BuildExplorationLine(ExplorationState state)
        {
            var equipment = EquipmentService.GetLeaderEquipmentSummary(state.Party);
            var status = state.IsPaused ? "일시정지" : state.IsExploring ? "탐험 중" : "대기";
            return
                $"{ZoneDefinitions.GetZoneDisplayName(state.ZoneId)} {state.CurrentFloor}층 · " +
                $"{status} · 장비 {equipment}";
        }

        public static string BuildSettingsSummary()
        {
            return
                $"{SkillTreeSystem.GetLeaderDisplayLabel()} · {BookshopSystem.GetDisplayLabel()} · " +
                $"{InnSystem.GetDisplayLabel()} · {BlacksmithSystem.GetDisplayLabel()} · " +
                $"{TrainingGroundSystem.GetDisplayLabel()} · {ScriptoriumSystem.GetDisplayLabel()} · " +
                $"{LlmQualitySettings.GetDisplayLabel()} · {LogFrequencySettings.GetDisplayLabel()} · " +
                $"{StageVfxDensitySettings.GetDisplayLabel()} · " +
                $"{OfflineSummaryDetailSettings.GetDisplayLabel()} · {DynamicEventAutoPolicySettings.GetDisplayLabel()} · " +
                $"{GoldenEventSettings.GetDisplayLabel()}";
        }

        public static string BuildStartCardSummary(ExplorationState state)
        {
            PrestigeManager.EnsureInitialized();
            var meta = PrestigeManager.GetMeta();
            var prestigeCount = Mathf.Max(1, (meta?.PrestigeCount ?? 0) + 1);
            var legacy = meta?.LegacyPoints ?? 0;
            var startingGold = PrestigeManager.GetStartingGoldBonus();
            var zoneName = ZoneDefinitions.GetZoneDisplayName(ZoneDefinitions.MossyHollowId);

            return
                $"<b>제 {prestigeCount}회차 길드</b>\n" +
                $"누적 유산 {legacy}\n" +
                $"시작 골드 +{startingGold}\n\n" +
                $"다음 목적지: {zoneName}\n" +
                "준비가 끝나면 아래 버튼을 눌러 탐험을 시작하세요.";
        }

        public static string BuildCenterStatusLine(ExplorationState state)
        {
            if (state == null || !state.IsExploring)
                return "탐험이 종료되었습니다.";

            if (state.IsPaused)
                return "일시정지 중 · R 또는 귀환 버튼으로 재개/귀환";

            if (DynamicEventSystem.IsAwaitingManualChoice)
                return "이벤트 선택 대기 · 1/2 키 또는 팝업에서 선택";

            return "자동 탐험 진행 중";
        }

        public static string BuildEnhancePanelText()
        {
            var leader = ExplorationSystem.GetCurrentState()?.Party?.Leader;
            if (leader == null)
                return "탐험 중인 파티가 없습니다.";

            var meta = PrestigeManager.GetMeta();
            var weaponLevel = EquipmentEnhanceSystem.GetEnhanceLevel(leader.CharacterId, EquipmentSlot.Weapon);
            var armorLevel = EquipmentEnhanceSystem.GetEnhanceLevel(leader.CharacterId, EquipmentSlot.Armor);
            var maxEnhance = EquipmentEnhanceSystem.GetMaxEnhanceLevel();
            var nextWeapon = EquipmentEnhanceSystem.GetEnhanceCost(weaponLevel + 1);
            var nextArmor = EquipmentEnhanceSystem.GetEnhanceCost(armorLevel + 1);
            var nextTier = CharacterTierSystem.GetPromoteCost(CharacterTierSystem.GetTierIndex(leader.CharacterId) + 1);

            return
                $"<b>{leader.DisplayName}</b> · {CharacterTierSystem.GetDisplayLabel(leader.CharacterId)}\n\n" +
                $"<b>장비</b>\n{EquipmentService.GetMemberEquipmentSummary(leader) ?? "장비 없음"}\n" +
                $"무기 +{weaponLevel}/{maxEnhance} · 방어구 +{armorLevel}/{maxEnhance}\n" +
                $"다음 강화 비용 — 무기: 유산 {nextWeapon.legacy}/마나 {nextWeapon.mana} · " +
                $"방어구: 유산 {nextArmor.legacy}/마나 {nextArmor.mana}\n\n" +
                $"<b>전직 비용 (다음)</b>\n명성 {nextTier.reputation} · 유산 {nextTier.legacy} · 유물 {nextTier.relic}\n\n" +
                $"<b>보유 자원</b>\n명성 {meta?.Reputation ?? 0} · 유산 {meta?.LegacyPoints ?? 0} · " +
                $"마나 {meta?.ManaShards ?? 0} · 유물 {meta?.RelicFragments ?? 0}\n" +
                $"대장간 {BlacksmithSystem.GetDisplayLabel()}";
        }

        public static string BuildFacilityPanelText() => BuildFacilitySummaryBlock();

        public static string BuildSettingsFacilityBlock() => BuildFacilitySummaryBlock(includeSkill: true);

        public static string GetRoleLabel(CharacterRole role) =>
            role switch
            {
                CharacterRole.Warrior => "전사",
                CharacterRole.Rogue => "도적",
                CharacterRole.Mage => "마법사",
                CharacterRole.Bard => "음유시인",
                CharacterRole.Cleric => "성직자",
                _ => role.ToString()
            };

        public static string GetInjuryLabel(InjurySeverity severity) =>
            severity switch
            {
                InjurySeverity.Light => "경상",
                InjurySeverity.Moderate => "중상",
                InjurySeverity.Severe => "중증",
                InjurySeverity.Fatal => "치명",
                _ => severity.ToString()
            };

        public static Color GetRoleTintColor(CharacterRole role) =>
            role switch
            {
                CharacterRole.Warrior => new Color(0.88f, 0.48f, 0.48f, 1f),
                CharacterRole.Rogue => new Color(0.62f, 0.83f, 0.62f, 1f),
                CharacterRole.Mage => new Color(0.43f, 0.77f, 1f, 1f),
                CharacterRole.Bard => new Color(1f, 0.85f, 0.4f, 1f),
                CharacterRole.Cleric => new Color(0.79f, 0.63f, 1f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f, 1f)
            };

        public static string BuildCardRoleLine(CharacterState member) =>
            $"Lv.{member.Level} {GetRoleLabel(member.Role)}";

        public static string BuildCardDetailLine(CharacterState member)
        {
            var equipment = EquipmentService.GetMemberEquipmentSummary(member);
            var injury = member.Injury != InjurySeverity.None ? GetInjuryLabel(member.Injury) : string.Empty;
            if (!string.IsNullOrEmpty(equipment) && !string.IsNullOrEmpty(injury))
                return $"{equipment} · {injury}";

            return !string.IsNullOrEmpty(equipment) ? equipment : injury;
        }

        public static string BuildCharacterDetailText(CharacterState member, PartyState party)
        {
            var builder = new StringBuilder();
            builder.Append("<b>[ ");
            builder.Append(member.DisplayName);
            builder.Append(" ]</b>  Lv.");
            builder.Append(member.Level);
            builder.Append(' ');
            builder.Append(GetRoleLabel(member.Role));
            builder.AppendLine();
            builder.AppendLine(CharacterTierSystem.GetTierTitle(member.CharacterId));

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
            builder.AppendLine(SkillTreeSystem.GetDisplayLabel(member.Role));

            builder.AppendLine();
            builder.AppendLine("<b>관계</b>");
            builder.AppendLine(BuildRelationshipLines(member, party));

            var memoryPreview = CharacterMemorySystem.BuildHudPreview(member.CharacterId);
            builder.AppendLine();
            builder.AppendLine("<b>최근 기억</b>");
            builder.AppendLine(string.IsNullOrEmpty(memoryPreview) ? "기록 없음" : memoryPreview);

            return builder.ToString();
        }

        private static string BuildFacilitySummaryBlock(bool includeSkill = false)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"<b>6. 필사가의 서고</b>\n{ScriptoriumSystem.GetDisplayLabel()}\n{ScriptoriumSystem.GetBonusSummary()}");
            builder.AppendLine();
            builder.AppendLine($"<b>7. 훈련소</b>\n{TrainingGroundSystem.GetDisplayLabel()}\n{TrainingGroundSystem.GetBonusSummary()}");
            builder.AppendLine();
            builder.AppendLine($"<b>8. 대장간</b>\n{BlacksmithSystem.GetDisplayLabel()}\n{BlacksmithSystem.GetBonusSummary()}");
            builder.AppendLine();
            builder.AppendLine($"<b>9. 여관</b>\n{InnSystem.GetDisplayLabel()}\n{InnSystem.GetBonusSummary()}");
            builder.AppendLine();
            builder.AppendLine($"<b>0. 서점</b>\n{BookshopSystem.GetDisplayLabel()}\n{BookshopSystem.GetBonusSummary()}");

            if (includeSkill)
            {
                builder.AppendLine();
                builder.AppendLine($"<b>-. 스킬 트리 (리더)</b>\n{SkillTreeSystem.GetLeaderDisplayLabel()}");
            }
            else
            {
                builder.AppendLine();
                builder.Append($"<b>-. 스킬 트리 (리더)</b>\n{SkillTreeSystem.GetLeaderDisplayLabel()}");
            }

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

                var affinity = RelationshipSystem.GetAffinity(member.CharacterId, other.CharacterId);
                var bond = affinity >= 60 ? " ★본드" : string.Empty;
                builder.Append(other.DisplayName);
                builder.Append(": ");
                builder.Append(affinity);
                builder.Append(bond);
                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }
    }
}
