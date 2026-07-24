using System;
using System.Collections.Generic;
using Backend.GameSystems.Character;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Simulation;
using Backend.GameSystems.Exploration.Stage;
using Backend.GameSystems.Save.Data;

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
            ExplorationStageSystem.Clear();
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

            if (DynamicEventSystem.HasActiveUnresolvedEvent)
                return new ExplorationTickResult();

            if (ExplorationStageSystem.IsBusy)
                return new ExplorationTickResult();

            var tickResult = _simulator.Tick(State, _random);
            PublishTickEvents(tickResult);

            var eventTriggered = false;
            if (State.CurrentFloor > _lastFloor)
            {
                eventTriggered = DynamicEventSystem.TryTriggerOnFloorEnter(State, _random, State.CurrentFloor);
                _lastFloor = State.CurrentFloor;
            }

            _ticksSinceLastDynamicEvent++;
            if (!eventTriggered &&
                !DynamicEventSystem.HasActiveUnresolvedEvent &&
                _ticksSinceLastDynamicEvent >= GuaranteedEventTickInterval)
            {
                eventTriggered = DynamicEventSystem.TryTriggerGuaranteed(State, _random);
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

            StageHighlightReplay.EnqueueOfflineHighlights(offlineResult.TopEvents, State.Party);

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
            ExplorationStageSystem.Clear();
            ExplorationChannels.PublishExplorationEnded(ExplorationEndReason.ManualReturn);
            ExplorationChannels.PublishStateChanged(State);
        }

        /// <summary>
        /// 진행 중 탐험 스냅샷을 내보낸다. 탐험 중이 아니면 null.
        /// </summary>
        public ExplorationRunSaveData ExportRunSave()
        {
            if (State == null || !State.IsExploring)
                return null;

            State.LastOnlineUtc = DateTime.UtcNow;
            return new ExplorationRunSaveData
            {
                HasActiveRun = true,
                State = CloneExplorationState(State),
                RandomState = _random?.ExportState() ?? (uint)Math.Max(State.Seed, 1),
                LastFloor = _lastFloor,
                TicksSinceLastDynamicEvent = _ticksSinceLastDynamicEvent
            };
        }

        /// <summary>
        /// 저장된 탐험 스냅샷으로 세션을 복원한다.
        /// </summary>
        public bool TryRestoreRun(ExplorationRunSaveData runSave)
        {
            if (runSave == null || !runSave.HasActiveRun || runSave.State == null || !runSave.State.IsExploring)
                return false;

            State = CloneExplorationState(runSave.State);
            _random = new DeterministicRandom(State.Seed);
            _random.ImportState(runSave.RandomState);
            _lastFloor = runSave.LastFloor > 0 ? runSave.LastFloor : State.CurrentFloor;
            _ticksSinceLastDynamicEvent = Math.Max(0, runSave.TicksSinceLastDynamicEvent);
            ExplorationStageSystem.Clear();
            ExplorationRollingSummary.Clear();
            CharacterMemorySystem.BindParty(State.Party);
            RelationshipSystem.BindParty(State.Party);
            ExplorationChannels.PublishStateChanged(State);
            return true;
        }

        private void PublishTickEvents(ExplorationTickResult tickResult)
        {
            var trivialCombatBatch = new List<ExplorationEvent>();

            foreach (var explorationEvent in tickResult.Events)
            {
                ExplorationRollingSummary.Record(explorationEvent, State.Party);
                if (!LogFrequencySettings.ShouldPublishLog(explorationEvent, State.Party))
                    continue;

                if (explorationEvent.EventType == EventType.CombatResult &&
                    explorationEvent.Salience <= SalienceGrade.Trivial)
                {
                    trivialCombatBatch.Add(explorationEvent);
                    continue;
                }

                FlushTrivialCombatBatch(trivialCombatBatch);
                EnqueueStageBeat(explorationEvent);
            }

            FlushTrivialCombatBatch(trivialCombatBatch);
        }

        private void FlushTrivialCombatBatch(List<ExplorationEvent> batch)
        {
            if (batch.Count == 0)
                return;

            if (batch.Count == 1)
            {
                EnqueueStageBeat(batch[0]);
                batch.Clear();
                return;
            }

            var events = batch.ToArray();
            batch.Clear();
            var combatBatch = new StageCombatBatch(events, State.Party, () =>
            {
                foreach (var explorationEvent in events)
                    PublishEventLog(explorationEvent);
            });
            ExplorationStageSystem.EnqueueCombatBatch(combatBatch);
        }

        private void EnqueueStageBeat(ExplorationEvent explorationEvent)
        {
            ExplorationStageSystem.Enqueue(explorationEvent, State.Party, () => PublishEventLog(explorationEvent));
        }

        private void PublishEventLog(ExplorationEvent explorationEvent)
        {
            var log = _narrator.Narrate(explorationEvent, State.Party);
            CharacterMemorySystem.RecordExplorationEvent(explorationEvent, State.Party);
            RelationshipSystem.RecordExplorationEvent(explorationEvent, State.Party);
            LoreCompendiumSystem.RecordDiscovery(explorationEvent);
            MonsterCompendiumSystem.RecordCombat(explorationEvent);
            ExplorationChannels.PublishLogAdded(log);
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
            if (source?.Members == null)
                return clone;

            foreach (var member in source.Members)
            {
                if (member == null)
                    continue;

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
                    PersonalityTags = member.PersonalityTags != null
                        ? new List<PersonalityTag>(member.PersonalityTags)
                        : new List<PersonalityTag>()
                });
            }

            return clone;
        }

        private static ExplorationState CloneExplorationState(ExplorationState source)
        {
            if (source == null)
                return null;

            return new ExplorationState
            {
                Seed = source.Seed,
                CurrentTick = source.CurrentTick,
                ZoneId = source.ZoneId,
                CurrentFloor = source.CurrentFloor,
                FloorProgress = source.FloorProgress,
                MaxFloor = source.MaxFloor,
                Party = CloneParty(source.Party),
                Gold = source.Gold,
                ManaShards = source.ManaShards,
                Reputation = source.Reputation,
                RelicFragments = source.RelicFragments,
                IsExploring = source.IsExploring,
                IsPaused = source.IsPaused,
                LastOnlineUtc = source.LastOnlineUtc
            };
        }
    }
}
