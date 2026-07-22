using Backend.GameSystems.Equipment;
using System.Collections.Generic;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Simulation
{
    public sealed class EventRollSystem
    {
        private readonly Dictionary<string, int> _recentEventCounts = new();

        public ExplorationEvent RollEvent(
            ExplorationState state,
            DeterministicRandom random,
            float partyHpRatio)
        {
            var categoryRoll = random.NextFloat();
            var restCutoff = partyHpRatio < 0.55f ? 0.86f : 0.88f;
            var eventType = categoryRoll switch
            {
                < 0.28f => EventType.Move,
                < 0.62f => EventType.CombatResult,
                < 0.78f => EventType.Discovery,
                < restCutoff => EventType.Rest,
                _ => EventType.Trap
            };

            return eventType switch
            {
                EventType.CombatResult => CreateCombatEvent(state, random, partyHpRatio),
                EventType.Discovery => CreateDiscoveryEvent(state, random, partyHpRatio),
                EventType.Rest => CreateRestEvent(state, partyHpRatio),
                EventType.Trap => CreateTrapEvent(state, random, partyHpRatio),
                _ => CreateMoveEvent(state, random, partyHpRatio)
            };
        }

        private ExplorationEvent CreateMoveEvent(
            ExplorationState state,
            DeterministicRandom random,
            float partyHpRatio)
        {
            var moveIds = ZoneDefinitions.GetMoveDescriptionIds(state.ZoneId);
            var moveId = moveIds[random.NextInt(moveIds.Count)];
            var salience = SalienceCalculator.Calculate(
                EventType.Move,
                null,
                false,
                partyHpRatio,
                GetRecentRepeatCount(EventType.Move));

            TrackEvent(EventType.Move);

            return new ExplorationEvent
            {
                EventId = BuildEventId(state),
                EventType = EventType.Move,
                Salience = salience,
                ZoneId = state.ZoneId,
                Floor = state.CurrentFloor,
                TimestampTick = state.CurrentTick,
                MoveDescriptionId = moveId,
                Actors = GetActorIds(state.Party)
            };
        }

        private ExplorationEvent CreateCombatEvent(
            ExplorationState state,
            DeterministicRandom random,
            float partyHpRatio)
        {
            var monster = RollMonster(state, random);
            var combat = CombatSimulator.Simulate(state.Party, monster, random, state.ZoneId, state.CurrentFloor);
            var salience = SalienceCalculator.Calculate(
                EventType.CombatResult,
                monster.Rarity,
                false,
                partyHpRatio,
                GetRecentRepeatCount(EventType.CombatResult));

            TrackEvent(EventType.CombatResult);
            ApplyCombatOutcome(state, combat);
            EquipmentService.TryProcessCombatDrop(state, combat, monster.Rarity, random);

            return new ExplorationEvent
            {
                EventId = BuildEventId(state),
                EventType = EventType.CombatResult,
                Salience = salience,
                ZoneId = state.ZoneId,
                Floor = state.CurrentFloor,
                TimestampTick = state.CurrentTick,
                Combat = combat,
                GoldDelta = combat.GoldGained,
                Actors = combat.Party
            };
        }

        private ExplorationEvent CreateDiscoveryEvent(
            ExplorationState state,
            DeterministicRandom random,
            float partyHpRatio)
        {
            var discoveries = ZoneDefinitions.GetDiscoveries(state.ZoneId);
            var discovery = discoveries[random.NextInt(discoveries.Count)];
            var salience = SalienceCalculator.Calculate(
                EventType.Discovery,
                null,
                true,
                partyHpRatio,
                GetRecentRepeatCount(EventType.Discovery));

            state.Gold += discovery.GoldValue;
            var manaGain = ZoneDefinitions.GetDiscoveryManaShards(discovery);
            state.ManaShards += manaGain;
            var relicGain = MetaCurrencyGrants.GrantDiscovery(state, discovery);
            TrackEvent(EventType.Discovery);

            return new ExplorationEvent
            {
                EventId = BuildEventId(state),
                EventType = EventType.Discovery,
                Salience = salience,
                ZoneId = state.ZoneId,
                Floor = state.CurrentFloor,
                TimestampTick = state.CurrentTick,
                DiscoveryItemId = discovery.ItemId,
                DiscoveryDisplayName = discovery.DisplayName,
                GoldDelta = discovery.GoldValue,
                ManaShardDelta = manaGain,
                RelicFragmentDelta = relicGain,
                Actors = GetActorIds(state.Party)
            };
        }

        private ExplorationEvent CreateRestEvent(ExplorationState state, float partyHpRatio)
        {
            InnManager.ApplyRest(state.Party);
            var salience = SalienceCalculator.Calculate(
                EventType.Rest,
                null,
                false,
                partyHpRatio,
                GetRecentRepeatCount(EventType.Rest));

            TrackEvent(EventType.Rest);

            return new ExplorationEvent
            {
                EventId = BuildEventId(state),
                EventType = EventType.Rest,
                Salience = salience,
                ZoneId = state.ZoneId,
                Floor = state.CurrentFloor,
                TimestampTick = state.CurrentTick,
                Actors = GetActorIds(state.Party)
            };
        }

        private ExplorationEvent CreateTrapEvent(
            ExplorationState state,
            DeterministicRandom random,
            float partyHpRatio)
        {
            var target = state.Party.Members[random.NextInt(state.Party.Members.Count)];
            var damage = random.NextInt(8) + 4;
            target.CurrentHp = System.Math.Max(1, target.CurrentHp - damage);
            target.Injury = InjurySeverity.Light;

            var salience = SalienceCalculator.Calculate(
                EventType.Trap,
                null,
                false,
                partyHpRatio,
                GetRecentRepeatCount(EventType.Trap));

            TrackEvent(EventType.Trap);

            return new ExplorationEvent
            {
                EventId = BuildEventId(state),
                EventType = EventType.Trap,
                Salience = salience,
                ZoneId = state.ZoneId,
                Floor = state.CurrentFloor,
                TimestampTick = state.CurrentTick,
                Actors = { target.CharacterId }
            };
        }

        private static ZoneDefinitions.MonsterDefinition RollMonster(ExplorationState state, DeterministicRandom random)
        {
            var monsters = ZoneDefinitions.GetMonsters(state.ZoneId);
            var roll = random.NextFloat();
            var index = state.CurrentFloor >= state.MaxFloor && roll < 0.08f
                ? monsters.Count - 1
                : roll switch
                {
                    < 0.55f => 0,
                    < 0.78f => 1,
                    < 0.92f => 2,
                    _ => 3
                };

            return ZoneDefinitions.ScaleMonsterForFloor(monsters[index], state.ZoneId, state.CurrentFloor);
        }

        private static void ApplyCombatOutcome(ExplorationState state, CombatResultPayload combat)
        {
            state.Gold += combat.GoldGained;

            foreach (var loot in combat.Loot)
            {
                var manaQuantity = ZoneDefinitions.GetManaShardQuantity(loot.ItemId) * loot.Quantity;
                state.ManaShards += manaQuantity;
            }

            if (combat.Outcome == CombatOutcome.Defeat)
            {
                state.IsExploring = false;
                return;
            }

            foreach (var pair in combat.ExpGained)
            {
                foreach (var member in state.Party.Members)
                {
                    if (member.CharacterId != pair.Key)
                        continue;

                    member.Level += pair.Value / 20;
                }
            }
        }

        private static List<string> GetActorIds(PartyState party)
        {
            var ids = new List<string>();
            foreach (var member in party.Members)
                ids.Add(member.CharacterId);

            return ids;
        }

        private static string BuildEventId(ExplorationState state)
        {
            return $"evt_{state.CurrentTick}_{state.CurrentFloor}";
        }

        private int GetRecentRepeatCount(EventType eventType)
        {
            return _recentEventCounts.TryGetValue(eventType.ToString(), out var count) ? count : 0;
        }

        private void TrackEvent(EventType eventType)
        {
            var key = eventType.ToString();
            _recentEventCounts.TryGetValue(key, out var count);
            _recentEventCounts[key] = count + 1;
        }
    }
}
