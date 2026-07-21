using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.DynamicEvent.Simulation;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Simulation;
using Backend.Util;
using Backend.Util.Management;
using UnityEngine;

namespace Backend.GameSystems.DynamicEvent
{
    /// <summary>
    /// 동적 이벤트(뼈대+LLM 연출) 발생·선택·결과 적용을 담당한다. Phase 4 기반 구현.
    /// </summary>
    public sealed class DynamicEventManager : SingletonGameObject<DynamicEventManager>
    {
        public DynamicEventInstance ActiveEvent { get; private set; }

        public static void EnsureInitialized()
        {
            if (GameStateUtil.IsQuitting)
                return;

            _ = Instance;
        }

        public static void TryTriggerOnFloorEnter(ExplorationState state, DeterministicRandom random, int enteredFloor)
        {
            if (GameStateUtil.IsQuitting || state == null)
                return;

            Instance.TryTriggerOnFloorEnterInternal(state, random, enteredFloor);
        }

        private void TryTriggerOnFloorEnterInternal(ExplorationState state, DeterministicRandom random, int enteredFloor)
        {
            if (GameStateUtil.IsQuitting || state == null || ActiveEvent != null)
                return;

            var template = DynamicEventRollSystem.TryRollFloorEnter(state.ZoneId, enteredFloor, random);
            if (template == null)
                return;

            var leader = state.Party?.Leader;
            ActiveEvent = new DynamicEventInstance
            {
                InstanceId = $"{template.EventId}_{state.CurrentTick}",
                TemplateId = template.EventId,
                ZoneId = state.ZoneId,
                Floor = enteredFloor,
                LeaderName = leader?.DisplayName ?? "탐험대"
            };

            DynamicEventChannels.PublishEventStarted(ActiveEvent);
            Debug.Log($"[DynamicEventManager] Started {template.EventId} at floor {enteredFloor}");

            AutoResolve(template, random, state);
        }

        private void AutoResolve(DynamicEventTemplate template, DeterministicRandom random, ExplorationState state)
        {
            var choiceId = ResolveAutoChoice(template, state);
            var outcome = DynamicEventResolver.ResolveChoice(template, choiceId, random);

            ActiveEvent.PlayerChoiceId = choiceId;
            ActiveEvent.ResolvedOutcome = outcome;
            ActiveEvent.IsResolved = true;
            ActiveEvent.LlmResultNarration = BuildTemplateResultText(template, choiceId, outcome);

            ApplyOutcome(state, outcome);
            DynamicEventChannels.PublishEventResolved(ActiveEvent);
            Debug.Log($"[DynamicEventManager] Resolved {template.EventId}: choice={choiceId}, outcome={outcome}");

            ActiveEvent = null;
        }

        private static string ResolveAutoChoice(DynamicEventTemplate template, ExplorationState state)
        {
            var leader = state.Party?.Leader;
            if (leader?.PersonalityTags != null)
            {
                foreach (var tag in leader.PersonalityTags)
                {
                    if (tag == PersonalityTag.Cautious && template.EventId == DynamicEventDefinitions.Fork002Id)
                        return "right_path";
                    if (tag == PersonalityTag.Reckless && template.EventId == DynamicEventDefinitions.Fork002Id)
                        return "left_path";
                }
            }

            return template.Choices.Count > 0 ? template.Choices[0].Id : null;
        }

        private static void ApplyOutcome(ExplorationState state, DynamicEventOutcomeEffect outcome)
        {
            switch (outcome)
            {
                case DynamicEventOutcomeEffect.MinorResource:
                case DynamicEventOutcomeEffect.GoldBonus:
                    state.Gold += 15;
                    break;
                case DynamicEventOutcomeEffect.MinorTrapDamage:
                case DynamicEventOutcomeEffect.InjuryLight:
                    if (state.Party?.Leader != null)
                        state.Party.Leader.CurrentHp = Mathf.Max(1, state.Party.Leader.CurrentHp - 8);
                    break;
            }
        }

        private static string BuildTemplateResultText(
            DynamicEventTemplate template,
            string choiceId,
            DynamicEventOutcomeEffect outcome)
        {
            return template.EventId switch
            {
                DynamicEventDefinitions.Fork002Id =>
                    choiceId == "left_path"
                        ? outcome == DynamicEventOutcomeEffect.MinorResource
                            ? "왼쪽 길 끝에서 작은 자원 더미를 발견했다."
                            : "왼쪽 길에서 함정에 발을 헛디뎌 경미한 부상을 입었다."
                        : outcome == DynamicEventOutcomeEffect.RareEncounter
                            ? "오른쪽 정적 속에서 예상치 못한 존재와 마주쳤다."
                            : "오른쪽 길은 고요했고, 특별한 일 없이 지나갔다.",
                _ => $"선택({choiceId})의 결과: {outcome}"
            };
        }
    }
}
