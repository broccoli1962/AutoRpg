using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Simulation
{
    public sealed class ExplorationSimulator
    {
        public const float TickDurationSeconds = 10f;
        public const int MaxOfflineHours = 12;

        private readonly EventRollSystem _eventRollSystem = new();

        public ExplorationTickResult Tick(ExplorationState state, DeterministicRandom random)
        {
            var result = new ExplorationTickResult();

            if (!state.IsExploring || state.IsPaused)
                return result;

            state.CurrentTick++;

            var progressGain = CalculateProgressGain(state);
            state.FloorProgress += progressGain;

            if (state.FloorProgress >= 100f)
            {
                state.FloorProgress = 0f;
                state.CurrentFloor++;

                var floorClearEvent = CreateFloorClearEvent(state);
                result.Events.Add(floorClearEvent);

                if (state.CurrentFloor > state.MaxFloor)
                {
                    if (ZoneDefinitions.TryAdvanceZone(state))
                    {
                        result.Events.Add(CreateZoneTransitionEvent(state));
                    }
                    else
                    {
                        state.IsExploring = false;
                        result.ExplorationEnded = true;
                        result.EndReason = ExplorationEndReason.ZoneComplete;
                    }
                }
            }

            var eventChance = ZoneDefinitions.BaseEventRollChance + state.CurrentFloor * 0.005f;
            if (random.RollChance(eventChance))
            {
                var partyHpRatio = CalculatePartyHpRatio(state.Party);
                var rolledEvent = _eventRollSystem.RollEvent(state, random, partyHpRatio);
                result.Events.Add(rolledEvent);

                if (rolledEvent.EventType == EventType.CombatResult &&
                    rolledEvent.Combat?.Outcome == CombatOutcome.Defeat)
                {
                    state.IsExploring = false;
                    result.ExplorationEnded = true;
                    result.EndReason = ExplorationEndReason.PartyDefeated;
                }
            }

            return result;
        }

        public OfflineSimulationResult SimulateOffline(ExplorationState state, DeterministicRandom random, long elapsedTicks)
        {
            var cappedTicks = System.Math.Min(elapsedTicks, MaxOfflineHours * 3600L / (long)TickDurationSeconds);
            var result = new OfflineSimulationResult
            {
                SimulatedTicks = cappedTicks
            };

            for (long i = 0; i < cappedTicks; i++)
            {
                var tickResult = Tick(state, random);
                result.AllEvents.AddRange(tickResult.Events);

                if (tickResult.ExplorationEnded)
                {
                    result.ExplorationEnded = true;
                    result.EndReason = tickResult.EndReason;
                    break;
                }
            }

            result.TopEvents = SelectTopSalientEvents(result.AllEvents, 8);
            return result;
        }

        public static float CalculateProgressGain(ExplorationState state)
        {
            var combatPower = 0f;
            var livingCount = 0;

            foreach (var member in state.Party.Members)
            {
                if (member.CurrentHp <= 0)
                    continue;

                livingCount++;
                combatPower += member.Str + member.Agi + member.Int + member.Vit * 0.5f;
            }

            if (livingCount == 0)
                return 0f;

            var averagePower = combatPower / livingCount;
            var baseRate = averagePower / 100f;
            var difficulty = ZoneDefinitions.GetFloorDifficulty(state.ZoneId, state.CurrentFloor);
            return ZoneDefinitions.BaseProgressPerTick * baseRate / difficulty;
        }

        public static float CalculatePartyHpRatio(PartyState party)
        {
            var current = 0;
            var max = 0;

            foreach (var member in party.Members)
            {
                current += member.CurrentHp;
                max += member.MaxHp;
            }

            return max <= 0 ? 0f : current / (float)max;
        }

        private static ExplorationEvent CreateFloorClearEvent(ExplorationState state)
        {
            return new ExplorationEvent
            {
                EventId = $"evt_floor_clear_{state.CurrentFloor - 1}",
                EventType = EventType.FloorClear,
                Salience = SalienceGrade.Milestone,
                ZoneId = state.ZoneId,
                Floor = state.CurrentFloor - 1,
                TimestampTick = state.CurrentTick,
                Actors = GetActorIds(state.Party)
            };
        }

        private static ExplorationEvent CreateZoneTransitionEvent(ExplorationState state)
        {
            return new ExplorationEvent
            {
                EventId = $"evt_zone_enter_{state.ZoneId}",
                EventType = EventType.ZoneTransition,
                Salience = SalienceGrade.Milestone,
                ZoneId = state.ZoneId,
                Floor = state.CurrentFloor,
                TimestampTick = state.CurrentTick,
                Actors = GetActorIds(state.Party)
            };
        }

        private static List<ExplorationEvent> SelectTopSalientEvents(List<ExplorationEvent> events, int count)
        {
            events.Sort((a, b) => GetSalienceScore(b.Salience).CompareTo(GetSalienceScore(a.Salience)));

            if (events.Count <= count)
                return new List<ExplorationEvent>(events);

            return events.GetRange(0, count);
        }

        private static int GetSalienceScore(SalienceGrade grade)
        {
            return grade switch
            {
                SalienceGrade.Milestone => 4,
                SalienceGrade.Significant => 3,
                SalienceGrade.Notable => 2,
                _ => 1
            };
        }

        private static List<string> GetActorIds(PartyState party)
        {
            var ids = new List<string>();
            foreach (var member in party.Members)
                ids.Add(member.CharacterId);

            return ids;
        }
    }

    public sealed class ExplorationTickResult
    {
        public List<ExplorationEvent> Events { get; } = new();
        public bool ExplorationEnded { get; set; }
        public ExplorationEndReason EndReason { get; set; }
    }

    public sealed class OfflineSimulationResult
    {
        public long SimulatedTicks { get; set; }
        public List<ExplorationEvent> AllEvents { get; } = new();
        public List<ExplorationEvent> TopEvents { get; set; } = new();
        public bool ExplorationEnded { get; set; }
        public ExplorationEndReason EndReason { get; set; }
    }

    public enum ExplorationEndReason
    {
        None,
        PartyDefeated,
        ZoneComplete,
        ManualReturn
    }
}
