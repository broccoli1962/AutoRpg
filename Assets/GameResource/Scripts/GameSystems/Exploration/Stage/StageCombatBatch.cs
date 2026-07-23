using System;
using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>연속 Trivial 전투를 하나의 스테이지 비트로 묶을 때 사용.</summary>
    public sealed class StageCombatBatch
    {
        public StageCombatBatch(IReadOnlyList<ExplorationEvent> events, PartyState party, Action publishLogs)
        {
            Events = events;
            Party = party;
            PublishLogs = publishLogs;
        }

        public IReadOnlyList<ExplorationEvent> Events { get; }
        public PartyState Party { get; }
        public Action PublishLogs { get; }

        public int Count => Events?.Count ?? 0;

        public int TotalGold
        {
            get
            {
                var sum = 0;
                if (Events == null)
                    return sum;

                foreach (var explorationEvent in Events)
                    sum += explorationEvent.Combat?.GoldGained ?? 0;

                return sum;
            }
        }

        public int TotalDamageDealt
        {
            get
            {
                var sum = 0;
                if (Events == null)
                    return sum;

                foreach (var explorationEvent in Events)
                    sum += explorationEvent.Combat?.DamageDealt ?? 0;

                return sum;
            }
        }

        public string PrimaryMonsterName
        {
            get
            {
                if (Events == null || Events.Count == 0)
                    return "몬스터";

                var name = Events[0].Combat?.MonsterDisplayName;
                return string.IsNullOrEmpty(name) ? "몬스터" : name;
            }
        }
    }
}
