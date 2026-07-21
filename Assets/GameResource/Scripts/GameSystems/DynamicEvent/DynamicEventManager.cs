using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.DynamicEvent.LLM;
using Backend.GameSystems.DynamicEvent.Simulation;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Simulation;
using Backend.Util;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.GameSystems.DynamicEvent
{
    /// <summary>
    /// 동적 이벤트(뼈대+LLM 연출) 발생·선택·결과 적용을 담당한다.
    /// </summary>
    public sealed class DynamicEventManager : SingletonGameObject<DynamicEventManager>
    {
        private const float SceneDisplaySeconds = 2.5f;

        private DynamicEventTemplate _pendingTemplate;
        private DeterministicRandom _pendingRandom;
        private ExplorationState _pendingState;
        private bool _isRunningEventFlow;

        public DynamicEventInstance ActiveEvent { get; private set; }

        public static bool HasActiveUnresolvedEvent =>
            !GameStateUtil.IsQuitting && Instance.ActiveEvent != null && !Instance.ActiveEvent.IsResolved;

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
            if (GameStateUtil.IsQuitting || state == null || ActiveEvent != null || _isRunningEventFlow)
                return;

            var template = DynamicEventRollSystem.TryRollFloorEnter(state.ZoneId, enteredFloor, random);
            if (template == null)
                return;

            var leader = state.Party?.Leader;
            _pendingTemplate = template;
            _pendingRandom = random;
            _pendingState = state;

            ActiveEvent = new DynamicEventInstance
            {
                InstanceId = $"{template.EventId}_{state.CurrentTick}",
                TemplateId = template.EventId,
                ZoneId = state.ZoneId,
                Floor = enteredFloor,
                LeaderName = leader?.DisplayName ?? "탐험대"
            };

            state.IsPaused = true;
            ExplorationChannels.PublishStateChanged(state);
            DynamicEventChannels.PublishEventStarted(ActiveEvent);
            Debug.Log($"[DynamicEventManager] Started {template.EventId} at floor {enteredFloor}");

            RunEventFlowAsync().Forget();
        }

        private async UniTaskVoid RunEventFlowAsync()
        {
            if (_pendingTemplate == null || _pendingState == null || ActiveEvent == null)
                return;

            _isRunningEventFlow = true;

            try
            {
                var sceneNarration = await DynamicEventLlmNarrator.TryGenerateSceneAsync(
                    _pendingTemplate,
                    _pendingState.Party,
                    ActiveEvent.Floor,
                    destroyCancellationToken);

                if (sceneNarration == null)
                {
                    sceneNarration = DynamicEventTemplateFallback.BuildScene(
                        _pendingTemplate,
                        ActiveEvent.LeaderName,
                        ActiveEvent.Floor);
                }

                ActiveEvent.LlmNarration = sceneNarration;
                ActiveEvent.IsSceneReady = true;
                DynamicEventChannels.PublishEventSceneReady(ActiveEvent);

                await UniTask.Delay(System.TimeSpan.FromSeconds(SceneDisplaySeconds), cancellationToken: destroyCancellationToken);

                var choiceId = ResolveAutoChoice(_pendingTemplate, _pendingState);
                var outcome = DynamicEventResolver.ResolveChoice(_pendingTemplate, choiceId, _pendingRandom);

                ActiveEvent.PlayerChoiceId = choiceId;
                ActiveEvent.ResolvedOutcome = outcome;

                var resultText = await DynamicEventLlmNarrator.TryGenerateResultAsync(
                    _pendingTemplate,
                    _pendingState.Party,
                    choiceId,
                    outcome,
                    destroyCancellationToken);

                if (string.IsNullOrWhiteSpace(resultText))
                {
                    resultText = DynamicEventTemplateFallback.BuildResult(_pendingTemplate, choiceId, outcome);
                }

                ActiveEvent.LlmResultNarration = resultText;
                ActiveEvent.IsResolved = true;

                ApplyOutcome(_pendingState, outcome);
                DynamicEventChannels.PublishEventResolved(ActiveEvent);
                Debug.Log(
                    $"[DynamicEventManager] Resolved {_pendingTemplate.EventId}: choice={choiceId}, outcome={outcome}");
            }
            finally
            {
                if (_pendingState != null)
                {
                    _pendingState.IsPaused = false;
                    ExplorationChannels.PublishStateChanged(_pendingState);
                }

                ActiveEvent = null;
                _pendingTemplate = null;
                _pendingRandom = null;
                _pendingState = null;
                _isRunningEventFlow = false;
            }
        }

        private static string ResolveAutoChoice(DynamicEventTemplate template, ExplorationState state)
        {
            var leader = state.Party?.Leader;
            if (leader?.PersonalityTags != null)
            {
                foreach (var tag in leader.PersonalityTags)
                {
                    switch (template.EventId)
                    {
                        case DynamicEventDefinitions.Fork002Id:
                        case DynamicEventDefinitions.ForkWaterSoundId:
                            if (tag == PersonalityTag.Cautious)
                                return GetSecondChoiceId(template);
                            if (tag == PersonalityTag.Reckless)
                                return GetFirstChoiceId(template);
                            break;
                        case DynamicEventDefinitions.EncounterMerchantId:
                            if (tag == PersonalityTag.Greedy)
                                return "trade";
                            if (tag == PersonalityTag.Cautious)
                                return "ignore";
                            break;
                        case DynamicEventDefinitions.TrapPressurePlateId:
                        case DynamicEventDefinitions.TrapPitId:
                        case DynamicEventDefinitions.HazardGasId:
                            if (tag == PersonalityTag.Cautious)
                                return GetSafeChoiceId(template);
                            if (tag == PersonalityTag.Reckless)
                                return GetRiskyChoiceId(template);
                            break;
                        case DynamicEventDefinitions.ArtifactMuralId:
                        case DynamicEventDefinitions.EncounterScholarId:
                            if (tag == PersonalityTag.Greedy || tag == PersonalityTag.Cheerful)
                                return GetFirstChoiceId(template);
                            if (tag == PersonalityTag.Cynical)
                                return GetSecondChoiceId(template);
                            break;
                    }
                }
            }

            return GetFirstChoiceId(template);
        }

        private static string GetFirstChoiceId(DynamicEventTemplate template) =>
            template.Choices.Count > 0 ? template.Choices[0].Id : null;

        private static string GetSecondChoiceId(DynamicEventTemplate template) =>
            template.Choices.Count > 1 ? template.Choices[1].Id : GetFirstChoiceId(template);

        private static string GetSafeChoiceId(DynamicEventTemplate template) =>
            template.EventId switch
            {
                DynamicEventDefinitions.TrapPressurePlateId => "step_back",
                DynamicEventDefinitions.HazardGasId => "retreat",
                DynamicEventDefinitions.TrapPitId => "climb",
                _ => GetSecondChoiceId(template)
            };

        private static string GetRiskyChoiceId(DynamicEventTemplate template) =>
            template.EventId switch
            {
                DynamicEventDefinitions.TrapPressurePlateId => "force_through",
                DynamicEventDefinitions.HazardGasId => "hold_breath",
                DynamicEventDefinitions.TrapPitId => "jump",
                _ => GetFirstChoiceId(template)
            };

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
    }
}
