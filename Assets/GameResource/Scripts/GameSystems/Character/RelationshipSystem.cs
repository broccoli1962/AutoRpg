using System.Collections.Generic;
using System.Text;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.Util;
using Backend.Util.Management;
using ExplorationEventType = global::EventType;
using R3;
using UnityEngine;

namespace Backend.GameSystems.Character
{
    /// <summary>
    /// 파티원 간 호감도/본드 관계를 관리하고 LLM 프롬프트·전투 시너지에 반영한다.
    /// </summary>
    public sealed class RelationshipSystem : SingletonGameObject<RelationshipSystem>
    {
        private const int BondThreshold = 60;
        private const float BondCombatMultiplier = 1.05f;

        private readonly Dictionary<string, int> _affinities = new();
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

        public static void BindParty(PartyState party)
        {
            if (GameStateUtil.IsQuitting || party?.Members == null)
                return;

            Instance.EnsurePairsExist(party);
        }

        public static void RecordExplorationEvent(ExplorationEvent explorationEvent, PartyState party)
        {
            if (GameStateUtil.IsQuitting || explorationEvent == null || party == null)
                return;

            Instance.RecordExplorationEventInternal(explorationEvent, party);
        }

        public static string BuildPartyPromptContext(PartyState party)
        {
            if (GameStateUtil.IsQuitting || party?.Leader == null)
                return string.Empty;

            return Instance.BuildPartyPromptContextInternal(party);
        }

        public static float GetBondCombatMultiplier(PartyState party)
        {
            if (GameStateUtil.IsQuitting || party == null)
                return 1f;

            return Instance.HasAnyBondInParty(party) ? BondCombatMultiplier : 1f;
        }

        public static Dictionary<string, int> ExportAffinities()
        {
            if (GameStateUtil.IsQuitting)
                return new Dictionary<string, int>();

            return new Dictionary<string, int>(Instance._affinities);
        }

        public static void ImportAffinities(Dictionary<string, int> affinities)
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance._affinities.Clear();
            if (affinities == null)
                return;

            foreach (var pair in affinities)
                Instance._affinities[pair.Key] = pair.Value;
        }

        public static int GetAffinity(string characterIdA, string characterIdB)
        {
            if (GameStateUtil.IsQuitting ||
                string.IsNullOrEmpty(characterIdA) ||
                string.IsNullOrEmpty(characterIdB))
            {
                return 0;
            }

            var key = BuildPairKey(characterIdA, characterIdB);
            return Instance._affinities.TryGetValue(key, out var affinity) ? affinity : 20;
        }

        public static string BuildHudSummary(PartyState party)
        {
            if (GameStateUtil.IsQuitting || party?.Members == null || party.Members.Count < 2)
                return null;

            var builder = new StringBuilder();
            builder.AppendLine("<color=#9ab0c8><b>[ 관계 ]</b></color>");

            for (var i = 0; i < party.Members.Count; i++)
            {
                for (var j = i + 1; j < party.Members.Count; j++)
                {
                    var a = party.Members[i];
                    var b = party.Members[j];
                    var affinity = GetAffinity(a.CharacterId, b.CharacterId);
                    var bond = affinity >= BondThreshold ? " ★" : string.Empty;

                    builder.Append(a.DisplayName);
                    builder.Append("↔");
                    builder.Append(b.DisplayName);
                    builder.Append(' ');
                    builder.Append(affinity);
                    builder.Append(bond);
                    builder.AppendLine();
                }
            }

            return builder.ToString().TrimEnd();
        }

        private void OnDynamicEventResolved(DynamicEventInstance instance)
        {
            if (instance == null)
                return;

            var party = ExplorationSystem.GetCurrentState()?.Party;
            if (party?.Members == null || party.Members.Count < 2)
                return;

            RecordDynamicEventInternal(instance, party);
        }

        private void RecordDynamicEventInternal(DynamicEventInstance instance, PartyState party)
        {
            EnsurePairsExist(party);

            var delta = ResolveDynamicEventAffinityDelta(instance);
            if (delta <= 0)
                return;

            AdjustAllPairs(party, delta);
            Debug.Log($"[RelationshipSystem] Dynamic event affinity +{delta} ({instance.TemplateId})");
        }

        private static int ResolveDynamicEventAffinityDelta(DynamicEventInstance instance)
        {
            var bonus = instance.TemplateId == DynamicEventDefinitions.GoldenChamberId ? 2 : 0;

            var baseDelta = instance.ResolvedOutcome switch
            {
                DynamicEventOutcomeEffect.GoldBonus => 3,
                DynamicEventOutcomeEffect.MinorResource => 2,
                DynamicEventOutcomeEffect.RareEncounter => 2,
                DynamicEventOutcomeEffect.SafePass => 1,
                DynamicEventOutcomeEffect.MinorTrapDamage => 1,
                DynamicEventOutcomeEffect.InjuryLight => 1,
                _ => 1
            };

            return baseDelta + bonus;
        }

        private void EnsurePairsExist(PartyState party)
        {
            for (var i = 0; i < party.Members.Count; i++)
            {
                for (var j = i + 1; j < party.Members.Count; j++)
                {
                    var key = BuildPairKey(party.Members[i].CharacterId, party.Members[j].CharacterId);
                    if (!_affinities.ContainsKey(key))
                        _affinities[key] = 20;
                }
            }
        }

        private void RecordExplorationEventInternal(ExplorationEvent explorationEvent, PartyState party)
        {
            EnsurePairsExist(party);

            switch (explorationEvent.EventType)
            {
                case ExplorationEventType.CombatResult when explorationEvent.Combat?.Outcome == CombatOutcome.Victory:
                    AdjustAllPairs(party, explorationEvent.Salience >= SalienceGrade.Significant ? 4 : 2);
                    break;
                case ExplorationEventType.Rest:
                    AdjustAllPairs(party, 2);
                    break;
                case ExplorationEventType.Trap:
                case ExplorationEventType.Injury:
                    AdjustAllPairs(party, 1);
                    break;
            }
        }

        private void AdjustAllPairs(PartyState party, int delta)
        {
            for (var i = 0; i < party.Members.Count; i++)
            {
                for (var j = i + 1; j < party.Members.Count; j++)
                {
                    var key = BuildPairKey(party.Members[i].CharacterId, party.Members[j].CharacterId);
                    _affinities.TryGetValue(key, out var current);
                    var next = Mathf.Clamp(current + delta, 0, 100);
                    var wasBond = current >= BondThreshold;
                    _affinities[key] = next;

                    if (!wasBond && next >= BondThreshold)
                    {
                        Debug.Log(
                            $"[RelationshipSystem] Bond unlocked: {party.Members[i].DisplayName} ↔ {party.Members[j].DisplayName}");
                    }
                }
            }
        }

        private bool HasAnyBondInParty(PartyState party)
        {
            for (var i = 0; i < party.Members.Count; i++)
            {
                for (var j = i + 1; j < party.Members.Count; j++)
                {
                    var key = BuildPairKey(party.Members[i].CharacterId, party.Members[j].CharacterId);
                    _affinities.TryGetValue(key, out var affinity);
                    if (affinity >= BondThreshold)
                        return true;
                }
            }

            return false;
        }

        private string BuildPartyPromptContextInternal(PartyState party)
        {
            var leader = party.Leader;
            var builder = new StringBuilder();
            builder.AppendLine("[파티 관계]");

            var hasEntry = false;
            foreach (var member in party.Members)
            {
                if (member.CharacterId == leader.CharacterId)
                    continue;

                var key = BuildPairKey(leader.CharacterId, member.CharacterId);
                _affinities.TryGetValue(key, out var affinity);
                if (affinity == 0 && !_affinities.ContainsKey(key))
                    affinity = 20;
                var bond = affinity >= BondThreshold ? " (동료 본드)" : string.Empty;

                builder.Append("- ");
                builder.Append(leader.DisplayName);
                builder.Append(" ↔ ");
                builder.Append(member.DisplayName);
                builder.Append(": 친밀 ");
                builder.Append(affinity);
                builder.AppendLine(bond);
                hasEntry = true;
            }

            return hasEntry ? builder.ToString() : string.Empty;
        }

        private static string BuildPairKey(string a, string b)
        {
            return string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
        }
    }
}
