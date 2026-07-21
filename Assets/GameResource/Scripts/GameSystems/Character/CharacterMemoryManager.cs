using System.Collections.Generic;
using System.Text;
using Backend.GameSystems.Character.Data;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.Util;
using Backend.Util.Management;
using R3;
using UnityEngine;

namespace Backend.GameSystems.Character
{
    /// <summary>
    /// 캐릭터별 단기/장기/핵심 기억을 관리하고 LLM 프롬프트용 컨텍스트를 제공한다.
    /// </summary>
    public sealed class CharacterMemoryManager : SingletonGameObject<CharacterMemoryManager>
    {
        private const int ShortTermCapacity = 8;
        private const int LongTermCompressThreshold = 6;

        private readonly Dictionary<string, CharacterMemory> _memories = new();
        private CompositeDisposable _disposables;

        public static void EnsureInitialized()
        {
            if (GameStateUtil.IsQuitting)
                return;

            _ = Instance;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            _disposables = new CompositeDisposable();

            DynamicEventChannels.OnEventResolved
                .Subscribe(OnDynamicEventResolved)
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        /// <summary>
        /// 탐험 시작 시 파티 캐릭터의 기억 슬롯을 준비한다.
        /// </summary>
        public static void BindParty(PartyState party)
        {
            if (GameStateUtil.IsQuitting || party?.Members == null)
                return;

            Instance.BindPartyInternal(party);
        }

        /// <summary>
        /// LLM 프롬프트에 주입할 캐릭터 기억 블록을 반환한다.
        /// </summary>
        public static string BuildPromptContext(string characterId)
        {
            if (GameStateUtil.IsQuitting || string.IsNullOrEmpty(characterId))
                return string.Empty;

            return Instance.BuildPromptContextInternal(characterId);
        }

        /// <summary>
        /// Significant+ 탐험 이벤트를 캐릭터 기억에 기록한다.
        /// </summary>
        public static void RecordExplorationEvent(ExplorationEvent explorationEvent, PartyState party)
        {
            if (GameStateUtil.IsQuitting || explorationEvent == null || party == null)
                return;

            if (explorationEvent.Salience < SalienceGrade.Significant)
                return;

            Instance.RecordExplorationEventInternal(explorationEvent, party);
        }

        private void BindPartyInternal(PartyState party)
        {
            foreach (var member in party.Members)
            {
                if (!_memories.ContainsKey(member.CharacterId))
                {
                    _memories[member.CharacterId] = new CharacterMemory
                    {
                        CharacterId = member.CharacterId
                    };
                }
            }
        }

        private void RecordExplorationEventInternal(ExplorationEvent explorationEvent, PartyState party)
        {
            BindPartyInternal(party);

            var leaderSummary = CharacterMemoryRecorder.SummarizeExplorationEvent(explorationEvent, party);
            if (!string.IsNullOrEmpty(leaderSummary) && party.Leader != null)
                AppendShortTerm(party.Leader.CharacterId, leaderSummary, party.Leader.DisplayName);

            foreach (var (characterId, summary) in CharacterMemoryRecorder.SummarizeCombatParticipants(explorationEvent, party))
            {
                var member = FindMember(party, characterId);
                AppendShortTerm(characterId, summary, member?.DisplayName);
            }

            TryAddCoreMemories(explorationEvent, party);
        }

        private void OnDynamicEventResolved(DynamicEventInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.LeaderName))
                return;

            var state = ExplorationManager.GetCurrentState();
            var party = state?.Party;
            if (party?.Leader == null)
                return;

            BindPartyInternal(party);
            var summary = CharacterMemoryRecorder.SummarizeDynamicEvent(instance);
            if (!string.IsNullOrEmpty(summary))
                AppendShortTerm(party.Leader.CharacterId, summary, party.Leader.DisplayName);
        }

        private void AppendShortTerm(string characterId, string summary, string displayName)
        {
            if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(summary))
                return;

            if (!_memories.TryGetValue(characterId, out var memory))
            {
                memory = new CharacterMemory { CharacterId = characterId };
                _memories[characterId] = memory;
            }

            memory.ShortTermBuffer.Add(summary);
            while (memory.ShortTermBuffer.Count > ShortTermCapacity)
                memory.ShortTermBuffer.RemoveAt(0);

            if (memory.ShortTermBuffer.Count >= LongTermCompressThreshold)
                memory.LongTermSummary = CharacterMemoryRecorder.BuildLongTermSummary(memory, displayName ?? characterId);

            Debug.Log($"[CharacterMemoryManager] Recorded for {characterId}: {summary}");
        }

        private void TryAddCoreMemories(ExplorationEvent explorationEvent, PartyState party)
        {
            if (explorationEvent.EventType != EventType.CombatResult || party?.Leader == null)
                return;

            var leader = party.Leader;
            if (!_memories.TryGetValue(leader.CharacterId, out var memory))
                return;

            var combat = explorationEvent.Combat;
            if (combat == null)
                return;

            if (HasCoreMemory(memory, "core_first_injury"))
                return;

            foreach (var injury in combat.Injuries)
            {
                if (injury.CharacterId != leader.CharacterId || injury.Severity == InjurySeverity.None)
                    continue;

                memory.CoreMemories.Add(new CoreMemoryEntry
                {
                    MemoryId = "core_first_injury",
                    Description = "첫 부상을 입은 순간",
                    Tags = { "injury", "first_time" },
                    Weight = "high"
                });
                Debug.Log($"[CharacterMemoryManager] Core memory unlocked: first injury ({leader.DisplayName})");
                break;
            }

            if (combat.Outcome == CombatOutcome.Victory &&
                explorationEvent.Salience >= SalienceGrade.Milestone &&
                !HasCoreMemory(memory, "core_boss_victory"))
            {
                memory.CoreMemories.Add(new CoreMemoryEntry
                {
                    MemoryId = "core_boss_victory",
                    Description = $"{combat.MonsterDisplayName ?? "강적"}을(를) 쓰러뜨린 전투",
                    Tags = { "victory", "milestone" },
                    Weight = "high"
                });
            }
        }

        private static bool HasCoreMemory(CharacterMemory memory, string memoryId)
        {
            foreach (var core in memory.CoreMemories)
            {
                if (core.MemoryId == memoryId)
                    return true;
            }

            return false;
        }

        private string BuildPromptContextInternal(string characterId)
        {
            if (!_memories.TryGetValue(characterId, out var memory))
                return string.Empty;

            var builder = new StringBuilder();
            builder.AppendLine("[캐릭터 기억]");

            if (!string.IsNullOrEmpty(memory.LongTermSummary))
            {
                builder.Append("장기: ");
                builder.AppendLine(memory.LongTermSummary);
            }

            if (memory.CoreMemories.Count > 0)
            {
                builder.Append("핵심: ");
                for (var i = 0; i < memory.CoreMemories.Count; i++)
                {
                    if (i > 0)
                        builder.Append("; ");

                    builder.Append(memory.CoreMemories[i].Description);
                }

                builder.AppendLine();
            }

            if (memory.ShortTermBuffer.Count > 0)
            {
                builder.AppendLine("최근:");
                var start = Mathf.Max(0, memory.ShortTermBuffer.Count - 3);
                for (var i = start; i < memory.ShortTermBuffer.Count; i++)
                {
                    builder.Append("- ");
                    builder.AppendLine(memory.ShortTermBuffer[i]);
                }
            }

            return builder.Length > 0 ? builder.ToString() : string.Empty;
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
    }
}
