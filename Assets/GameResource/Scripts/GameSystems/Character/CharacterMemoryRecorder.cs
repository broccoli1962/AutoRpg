using System.Collections.Generic;
using System.Text;
using Backend.GameSystems.Character.Data;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Character
{
    /// <summary>
    /// 탐험/동적 이벤트에서 캐릭터 기억 요약 문장을 생성한다.
    /// </summary>
    public static class CharacterMemoryRecorder
    {
        public static string SummarizeExplorationEvent(ExplorationEvent explorationEvent, PartyState party)
        {
            if (explorationEvent == null)
                return null;

            var leader = party?.Leader;
            var leaderName = leader?.DisplayName ?? "탐험대";

            return explorationEvent.EventType switch
            {
                EventType.CombatResult => SummarizeCombat(explorationEvent, leaderName),
                EventType.Discovery =>
                    $"{leaderName}(은)는 {explorationEvent.Floor}층에서 {explorationEvent.DiscoveryDisplayName ?? "보물"}(을)를 발견했다.",
                EventType.FloorClear =>
                    $"{leaderName}(은)는 {explorationEvent.Floor}층을 돌파했다.",
                EventType.Injury or EventType.Trap =>
                    $"{leaderName}(은)는 {explorationEvent.Floor}층에서 부상을 입었다.",
                EventType.Death =>
                    $"{leaderName}(은)는 {explorationEvent.Floor}층에서 위기에 처했다.",
                _ => null
            };
        }

        public static string SummarizeDynamicEvent(DynamicEventInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.LlmResultNarration))
                return null;

            return $"{instance.LeaderName}(은)는 {instance.Floor}층 이벤트({instance.TemplateId})에서 \"{instance.LlmResultNarration}\"";
        }

        public static string SummarizeDynamicEventForMember(DynamicEventInstance instance, CharacterState member)
        {
            if (instance == null || member == null || string.IsNullOrEmpty(instance.LlmResultNarration))
                return null;

            var subject = member.DisplayName ?? member.CharacterId;
            if (instance.LeaderName == subject)
                return SummarizeDynamicEvent(instance);

            return
                $"{subject}(은)는 {instance.Floor}층에서 {instance.LeaderName}(와)과 함께 이벤트({instance.TemplateId})를 경험했다.";
        }

        public static IEnumerable<(string CharacterId, string Summary)> SummarizeCombatParticipants(
            ExplorationEvent explorationEvent,
            PartyState party)
        {
            var combat = explorationEvent?.Combat;
            if (combat?.Injuries == null || party?.Members == null)
                yield break;

            foreach (var injury in combat.Injuries)
            {
                if (injury.Severity == InjurySeverity.None)
                    continue;

                var member = FindMember(party, injury.CharacterId);
                if (member == null)
                    continue;

                yield return (
                    member.CharacterId,
                    $"{member.DisplayName}(은)는 {combat.MonsterDisplayName ?? "적"}과의 전투에서 부상을 입었다.");
            }
        }

        private static string SummarizeCombat(ExplorationEvent explorationEvent, string leaderName)
        {
            var combat = explorationEvent.Combat;
            if (combat == null)
                return null;

            return combat.Outcome switch
            {
                CombatOutcome.Victory =>
                    $"{leaderName}(은)는 {combat.MonsterDisplayName ?? "적"}을(를) 격퇴했다.",
                CombatOutcome.Defeat =>
                    $"{leaderName}(은)는 {combat.MonsterDisplayName ?? "적"}에게 패배했다.",
                CombatOutcome.Retreat =>
                    $"{leaderName}(은)는 {combat.MonsterDisplayName ?? "적"}과의 전투에서 후퇴했다.",
                _ => null
            };
        }

        private static CharacterState FindMember(PartyState party, string characterId)
        {
            foreach (var member in party.Members)
            {
                if (member.CharacterId == characterId)
                    return member;
            }

            return null;
        }

        public static string BuildLongTermSummary(CharacterMemory memory, string displayName)
        {
            if (memory?.ShortTermBuffer == null || memory.ShortTermBuffer.Count == 0)
                return null;

            var injuryCount = 0;
            var victoryCount = 0;
            foreach (var entry in memory.ShortTermBuffer)
            {
                if (entry.Contains("부상"))
                    injuryCount++;
                if (entry.Contains("격퇴"))
                    victoryCount++;
            }

            var builder = new StringBuilder();
            builder.Append(displayName);
            builder.Append("(은)는 최근 탐험에서 ");

            if (injuryCount >= 2)
                builder.Append("여러 번 위험에 처하며 더 신중해졌고, ");
            if (victoryCount >= 2)
                builder.Append("연속 전투 승리로 자신감을 얻었으며, ");

            builder.Append("동굴 깊숙한 곳으로 향하고 있다.");
            return builder.ToString();
        }
    }
}
