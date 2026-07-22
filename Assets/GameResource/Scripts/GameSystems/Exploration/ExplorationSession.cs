using System;
using System.Collections.Generic;
using Backend.GameSystems.Character;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Simulation;

namespace Backend.GameSystems.Exploration
{
    public sealed class ExplorationSession
    {
        private const int GuaranteedEventTickInterval = 90;

        private readonly ExplorationSimulator _simulator = new();
        private readonly ILogNarrator _narrator;
        private DeterministicRandom _random;
        private int _lastFloor = 1;
        private int _ticksSinceLastDynamicEvent;

        public ExplorationState State { get; private set; }

        public ExplorationSession(ILogNarrator narrator)
        {
            _narrator = narrator;
        }

        public void StartNew(int seed, PartyState party, string zoneId = ZoneDefinitions.MossyHollowId)
        {
            _random = new DeterministicRandom(seed);
            _lastFloor = 1;
            _ticksSinceLastDynamicEvent = 0;
            ExplorationRollingSummary.Clear();
            State = new ExplorationState
            {
                Seed = seed,
                ZoneId = zoneId,
                CurrentFloor = 1,
                MaxFloor = ZoneDefinitions.GetMaxFloor(zoneId),
                Party = CloneParty(party),
                IsExploring = true,
                IsPaused = false,
                LastOnlineUtc = DateTime.UtcNow
            };
        }

        public ExplorationTickResult ProcessTick()
        {
            if (State == null || !State.IsExploring || State.IsPaused)
                return new ExplorationTickResult();

            if (DynamicEventManager.HasActiveUnresolvedEvent)
                return new ExplorationTickResult();

            var tickResult = _simulator.Tick(State, _random);
            PublishTickEvents(tickResult);

            var eventTriggered = false;
            if (State.CurrentFloor > _lastFloor)
            {
                eventTriggered = DynamicEventManager.TryTriggerOnFloorEnter(State, _random, State.CurrentFloor);
                _lastFloor = State.CurrentFloor;
            }

            _ticksSinceLastDynamicEvent++;
            if (!eventTriggered &&
                !DynamicEventManager.HasActiveUnresolvedEvent &&
                _ticksSinceLastDynamicEvent >= GuaranteedEventTickInterval)
            {
                eventTriggered = DynamicEventManager.TryTriggerGuaranteed(State, _random);
            }

            if (eventTriggered)
                _ticksSinceLastDynamicEvent = 0;

            if (tickResult.ExplorationEnded)
                ExplorationChannels.PublishExplorationEnded(tickResult.EndReason);

            ExplorationChannels.PublishStateChanged(State);
            return tickResult;
        }

        public OfflineSimulationResult ProcessOffline(TimeSpan elapsed)
        {
            if (State == null)
                return new OfflineSimulationResult();

            var startFloor = State.CurrentFloor;
            var startGold = State.Gold;
            var elapsedTicks = (long)(elapsed.TotalSeconds / ExplorationSimulator.TickDurationSeconds);
            var offlineResult = _simulator.SimulateOffline(State, _random, elapsedTicks);

            foreach (var topEvent in offlineResult.TopEvents)
            {
                if (!LogFrequencySettings.ShouldPublishLog(topEvent, State.Party))
                    continue;

                var log = _narrator.Narrate(topEvent, State.Party);
                ExplorationChannels.PublishLogAdded(log);
            }

            var summary = _narrator.NarrateOfflineSummary(new OfflineSummaryContext
            {
                SimulatedTicks = offlineResult.SimulatedTicks,
                EventCount = offlineResult.AllEvents.Count,
                GoldGained = State.Gold - startGold,
                StartFloor = startFloor,
                EndFloor = State.CurrentFloor,
                ZoneDisplayName = ZoneDefinitions.GetZoneDisplayName(State.ZoneId),
                CombatCount = CountEvents(offlineResult.AllEvents, EventType.CombatResult),
                DiscoveryCount = CountEvents(offlineResult.AllEvents, EventType.Discovery),
                MilestoneCount = CountMilestones(offlineResult.AllEvents)
            });

            ExplorationChannels.PublishLogAdded(summary);

            if (offlineResult.ExplorationEnded)
                ExplorationChannels.PublishExplorationEnded(offlineResult.EndReason);

            State.LastOnlineUtc = DateTime.UtcNow;
            ExplorationChannels.PublishStateChanged(State);
            return offlineResult;
        }

        public void Pause()
        {
            if (State == null)
                return;

            State.IsPaused = true;
            ExplorationChannels.PublishStateChanged(State);
        }

        public void Resume()
        {
            if (State == null)
                return;

            State.IsPaused = false;
            State.LastOnlineUtc = DateTime.UtcNow;
            ExplorationChannels.PublishStateChanged(State);
        }

        public void ReturnToGuild()
        {
            if (State == null)
                return;

            State.IsExploring = false;
            ExplorationChannels.PublishExplorationEnded(ExplorationEndReason.ManualReturn);
            ExplorationChannels.PublishStateChanged(State);
        }

        private void PublishTickEvents(ExplorationTickResult tickResult)
        {
            foreach (var explorationEvent in tickResult.Events)
            {
                ExplorationRollingSummary.Record(explorationEvent, State.Party);
                if (!LogFrequencySettings.ShouldPublishLog(explorationEvent, State.Party))
                    continue;

                var log = _narrator.Narrate(explorationEvent, State.Party);
                CharacterMemoryManager.RecordExplorationEvent(explorationEvent, State.Party);
                RelationshipManager.RecordExplorationEvent(explorationEvent, State.Party);
                LoreCompendiumManager.RecordDiscovery(explorationEvent);
                MonsterCompendiumManager.RecordCombat(explorationEvent);
                ExplorationChannels.PublishLogAdded(log);
            }
        }

        private static int CountEvents(IReadOnlyList<ExplorationEvent> events, EventType eventType)
        {
            var count = 0;
            foreach (var explorationEvent in events)
            {
                if (explorationEvent.EventType == eventType)
                    count++;
            }

            return count;
        }

        private static int CountMilestones(IReadOnlyList<ExplorationEvent> events)
        {
            var count = 0;
            foreach (var explorationEvent in events)
            {
                if (explorationEvent.Salience >= SalienceGrade.Milestone ||
                    explorationEvent.EventType == EventType.FloorClear ||
                    explorationEvent.EventType == EventType.ZoneTransition)
                {
                    count++;
                }
            }

            return count;
        }

        private static PartyState CloneParty(PartyState source)
        {
            var clone = new PartyState();
            foreach (var member in source.Members)
            {
                clone.Members.Add(new CharacterState
                {
                    CharacterId = member.CharacterId,
                    DisplayName = member.DisplayName,
                    Role = member.Role,
                    Level = member.Level,
                    Str = member.Str,
                    Agi = member.Agi,
                    Int = member.Int,
                    Vit = member.Vit,
                    Luk = member.Luk,
                    CurrentHp = member.CurrentHp,
                    MaxHp = member.MaxHp,
                    Injury = member.Injury,
                    EquippedWeaponId = member.EquippedWeaponId,
                    EquippedArmorId = member.EquippedArmorId,
                    WeaponEnhanceLevel = member.WeaponEnhanceLevel,
                    ArmorEnhanceLevel = member.ArmorEnhanceLevel,
                    PersonalityTags = new List<PersonalityTag>(member.PersonalityTags)
                });
            }

            return clone;
        }
    }
}
