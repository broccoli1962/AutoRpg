using System;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>스테이지 연출 단위 — 시뮬 이벤트 1건에 대응.</summary>
    public enum StageBeatKind
    {
        Move,
        Combat,
        Discovery,
        Rest,
        Trap,
        Milestone
    }

    /// <summary>Stage Director 큐 항목. 연출 완료 후 PublishLog 콜백으로 로그를 발행한다.</summary>
    public sealed class StageBeatRequest
    {
        public StageBeatRequest(ExplorationEvent explorationEvent, PartyState party, Action publishLog)
        {
            Event = explorationEvent;
            Party = party;
            PublishLog = publishLog;
        }

        public StageBeatRequest(StageCombatBatch batch)
        {
            CombatBatch = batch ?? throw new ArgumentNullException(nameof(batch));
            Event = batch.Events[0];
            Party = batch.Party;
            PublishLog = batch.PublishLogs;
        }

        public ExplorationEvent Event { get; }
        public PartyState Party { get; }
        public Action PublishLog { get; }
        public StageCombatBatch CombatBatch { get; }

        public bool IsCombatBatch => CombatBatch != null && CombatBatch.Count > 1;

        public StageBeatKind Kind => MapKind(Event?.EventType ?? EventType.Move);

        public static StageBeatKind MapKind(EventType eventType)
        {
            return eventType switch
            {
                EventType.CombatResult => StageBeatKind.Combat,
                EventType.Discovery => StageBeatKind.Discovery,
                EventType.Rest => StageBeatKind.Rest,
                EventType.Trap => StageBeatKind.Trap,
                EventType.FloorClear or EventType.ZoneTransition or EventType.Death => StageBeatKind.Milestone,
                _ => StageBeatKind.Move
            };
        }
    }
}
