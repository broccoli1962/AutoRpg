using System.Collections.Generic;
using System.Text;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Simulation;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// Phase 1 템플릿 기반 내레이터. Salience 등급에 따라 변수 치환 문장을 생성한다.
    /// </summary>
    public sealed class TemplateLogNarrator : ILogNarrator
    {
        private static readonly Dictionary<string, string> MoveTemplates = new()
        {
            { "damp_passage", "{leader}는 습기가 배어 있는 통로를 조심스럽게 지나갔다." },
            { "glowing_moss", "벽면의 발광 이끼가 {leader}의 발걸음을 희미하게 비추었다." },
            { "narrow_tunnel", "{leader}는 좁아진 터널 속에서 숨을 고르며 전진했다." },
            { "water_drip", "천장에서 떨어지는 물방울 소리가 동굴을 울렸다. {leader}는 방향을 확인했다." },
            { "fungal_scent", "곰팡이 냄새가 짙어지자 {leader}는 마스크를 고쳐 썼다." }
        };

        public LogEntry Narrate(ExplorationEvent explorationEvent, PartyState party)
        {
            var leader = party.Leader;
            var leaderName = leader?.DisplayName ?? "탐험대";
            var text = explorationEvent.EventType switch
            {
                EventType.Move => FormatMove(explorationEvent, leaderName),
                EventType.CombatResult => FormatCombat(explorationEvent, leaderName),
                EventType.Discovery => FormatDiscovery(explorationEvent, leaderName),
                EventType.Rest => $"{leaderName} 일행은 잠시 자리를 잡고 숨을 고르며 상처를 돌보았다.",
                EventType.Trap => $"{leaderName} 일행은 미끄러운 바닥에 발을 헛디뎌 경미한 부상을 입었다.",
                EventType.FloorClear => $"{ZoneDefinitions.GetZoneDisplayName(explorationEvent.ZoneId)} {explorationEvent.Floor}층을 돌파했다.",
                EventType.OfflineSummary => "오프라인 탐험 요약",
                _ => $"{leaderName} 일행에게 예상치 못한 일이 벌어졌다."
            };

            return new LogEntry
            {
                EventId = explorationEvent.EventId,
                EventType = explorationEvent.EventType,
                Salience = explorationEvent.Salience,
                Category = MapCategory(explorationEvent.EventType),
                Text = text,
                TimestampTick = explorationEvent.TimestampTick,
                Floor = explorationEvent.Floor
            };
        }

        public LogEntry NarrateOfflineSummary(OfflineSummaryContext context)
        {
            var hours = context.SimulatedTicks * ExplorationSimulator.TickDurationSeconds / 3600f;
            var builder = new StringBuilder();
            builder.Append("[지난 ");
            builder.Append(hours.ToString("0.#"));
            builder.Append("시간의 기록]\n\n");
            builder.Append("파티는 ");
            builder.Append(context.ZoneDisplayName);
            builder.Append(' ');
            builder.Append(context.StartFloor);
            builder.Append("층에서 ");
            builder.Append(context.EndFloor);
            builder.Append("층까지 나아갔다. ");
            builder.Append("그 사이 ");
            builder.Append(context.EventCount);
            builder.Append("건의 사건이 기록되었고, 골드 ");
            builder.Append(context.GoldGained);
            builder.Append("G를 챙겼다.");

            return new LogEntry
            {
                EventId = "offline_summary",
                EventType = EventType.OfflineSummary,
                Salience = SalienceGrade.Significant,
                Category = LogCategory.Milestone,
                Text = builder.ToString(),
                TimestampTick = 0,
                Floor = context.EndFloor
            };
        }

        private static string FormatMove(ExplorationEvent explorationEvent, string leaderName)
        {
            if (explorationEvent.MoveDescriptionId != null &&
                MoveTemplates.TryGetValue(explorationEvent.MoveDescriptionId, out var template))
            {
                return template.Replace("{leader}", leaderName);
            }

            return $"{leaderName}는 어두운 통로를 따라 전진했다.";
        }

        private static string FormatCombat(ExplorationEvent explorationEvent, string leaderName)
        {
            var combat = explorationEvent.Combat;
            if (combat == null)
                return $"{leaderName} 일행은 전투를 치렀다.";

            var monsterName = combat.MonsterDisplayName ?? "적";
            var exp = 0;
            foreach (var pair in combat.ExpGained)
            {
                exp = pair.Value;
                break;
            }

            return combat.Outcome switch
            {
                CombatOutcome.Victory =>
                    $"{leaderName}가 {monsterName}을(를) 처치했다. (+{combat.GoldGained}G, +{exp}XP)",
                CombatOutcome.Retreat =>
                    $"{leaderName} 일행은 {monsterName}과(와)의 전투가 길어져 안전한 지점으로 후퇴했다.",
                CombatOutcome.Defeat =>
                    $"{leaderName} 일행은 {monsterName}에게 밀려 탐험을 중단하고 귀환길에 올랐다.",
                _ => $"{leaderName} 일행은 {monsterName}과(와) 교전했다."
            };
        }

        private static string FormatDiscovery(ExplorationEvent explorationEvent, string leaderName)
        {
            var itemName = explorationEvent.DiscoveryDisplayName ?? "보물";
            return $"{leaderName}는 바위틈에서 {itemName}을(를) 발견했다. (+{explorationEvent.GoldDelta}G)";
        }

        private static LogCategory MapCategory(EventType eventType)
        {
            return eventType switch
            {
                EventType.Move => LogCategory.Move,
                EventType.CombatResult => LogCategory.Combat,
                EventType.Discovery => LogCategory.Discovery,
                EventType.Rest or EventType.Injury or EventType.Trap => LogCategory.Status,
                EventType.FloorClear or EventType.OfflineSummary or EventType.Death => LogCategory.Milestone,
                _ => LogCategory.Move
            };
        }
    }
}
