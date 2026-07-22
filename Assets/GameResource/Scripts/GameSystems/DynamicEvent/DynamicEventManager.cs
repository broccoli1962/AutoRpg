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

        public static bool TryTriggerOnFloorEnter(ExplorationState state, DeterministicRandom random, int enteredFloor)
        {
            if (GameStateUtil.IsQuitting || state == null)
                return false;

            return Instance.TryStartEvent(state, random, enteredFloor, useProbabilityRoll: true);
        }

        /// <summary>
        /// Tick 간격 보장용 — 확률 없이 현재 층에서 발생 가능한 이벤트를 강제 롤한다.
        /// </summary>
        public static bool TryTriggerGuaranteed(ExplorationState state, DeterministicRandom random)
        {
            if (GameStateUtil.IsQuitting || state == null)
                return false;

            return Instance.TryStartEvent(state, random, state.CurrentFloor, useProbabilityRoll: false);
        }

        private bool TryStartEvent(
            ExplorationState state,
            DeterministicRandom random,
            int floor,
            bool useProbabilityRoll)
        {
            if (GameStateUtil.IsQuitting || state == null || ActiveEvent != null || _isRunningEventFlow)
                return false;

            var template = useProbabilityRoll
                ? DynamicEventRollSystem.TryRollFloorEnter(state.ZoneId, floor, random)
                : DynamicEventRollSystem.RollGuaranteed(state.ZoneId, floor, random);

            if (template == null)
                return false;

            var leader = state.Party?.Leader;
            _pendingTemplate = template;
            _pendingRandom = random;
            _pendingState = state;

            ActiveEvent = new DynamicEventInstance
            {
                InstanceId = $"{template.EventId}_{state.CurrentTick}",
                TemplateId = template.EventId,
                ZoneId = state.ZoneId,
                Floor = floor,
                LeaderName = leader?.DisplayName ?? "탐험대"
            };

            state.IsPaused = true;
            ExplorationChannels.PublishStateChanged(state);
            DynamicEventChannels.PublishEventStarted(ActiveEvent);
            Debug.Log(
                $"[DynamicEventManager] Started {template.EventId} at floor {floor} (guaranteed={!useProbabilityRoll})");

            RunEventFlowAsync().Forget();
            return true;
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
            return DynamicEventAutoPolicySettings.Current switch
            {
                DynamicEventAutoPolicy.Safe => GetSafeChoiceId(template),
                DynamicEventAutoPolicy.Adventure => GetRiskyChoiceId(template),
                DynamicEventAutoPolicy.Greedy => GetGreedyChoiceId(template),
                _ => ResolvePersonalityChoice(template, state)
            };
        }

        private static string ResolvePersonalityChoice(DynamicEventTemplate template, ExplorationState state)
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
                        case DynamicEventDefinitions.ForkRuneMarkId:
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
                        case DynamicEventDefinitions.EncounterFairyId:
                            if (tag == PersonalityTag.Greedy || tag == PersonalityTag.Cheerful)
                                return "accept_gift";
                            if (tag == PersonalityTag.Cautious || tag == PersonalityTag.Cynical)
                                return "decline";
                            break;
                        case DynamicEventDefinitions.TrapPressurePlateId:
                        case DynamicEventDefinitions.TrapPitId:
                        case DynamicEventDefinitions.HazardGasId:
                        case DynamicEventDefinitions.HazardCollapseId:
                        case DynamicEventDefinitions.HazardQuicksandId:
                            if (tag == PersonalityTag.Cautious)
                                return GetSafeChoiceId(template);
                            if (tag == PersonalityTag.Reckless)
                                return GetRiskyChoiceId(template);
                            break;
                        case DynamicEventDefinitions.ArtifactMuralId:
                        case DynamicEventDefinitions.ArtifactLoreFragmentId:
                        case DynamicEventDefinitions.EncounterScholarId:
                        case DynamicEventDefinitions.EncounterWandererId:
                            if (tag == PersonalityTag.Greedy || tag == PersonalityTag.Cheerful)
                                return GetFirstChoiceId(template);
                            if (tag == PersonalityTag.Cynical)
                                return GetSecondChoiceId(template);
                            if (tag == PersonalityTag.Loyal && template.EventId == DynamicEventDefinitions.EncounterWandererId)
                                return "help";
                            break;
                        case DynamicEventDefinitions.ArtifactCrystalId:
                            if (tag == PersonalityTag.Greedy)
                                return "take";
                            if (tag == PersonalityTag.Cautious)
                                return "leave";
                            break;
                    }
                }
            }

            return GetFirstChoiceId(template);
        }

        private static string GetGreedyChoiceId(DynamicEventTemplate template) =>
            template.EventId switch
            {
                DynamicEventDefinitions.EncounterMerchantId => "trade",
                DynamicEventDefinitions.EncounterFairyId => "accept_gift",
                DynamicEventDefinitions.ArtifactCrystalId => "take",
                DynamicEventDefinitions.ArtifactLoreFragmentId => "read",
                DynamicEventDefinitions.ArtifactMuralId => "study",
                DynamicEventDefinitions.EncounterScholarId => "talk",
                DynamicEventDefinitions.EncounterWandererId => "help",
                _ => GetFirstChoiceId(template)
            };

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
                DynamicEventDefinitions.HazardCollapseId => "cover",
                DynamicEventDefinitions.HazardQuicksandId => "wait",
                _ => GetSecondChoiceId(template)
            };

        private static string GetRiskyChoiceId(DynamicEventTemplate template) =>
            template.EventId switch
            {
                DynamicEventDefinitions.TrapPressurePlateId => "force_through",
                DynamicEventDefinitions.HazardGasId => "hold_breath",
                DynamicEventDefinitions.TrapPitId => "jump",
                DynamicEventDefinitions.HazardCollapseId => "dash",
                DynamicEventDefinitions.HazardQuicksandId => "pull_free",
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
