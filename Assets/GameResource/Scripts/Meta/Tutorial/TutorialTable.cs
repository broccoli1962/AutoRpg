using System;
using System.Collections.Generic;
using UnityEngine;

namespace Backend.Meta.Tutorial
{
    /// <summary>
    /// FTUE 6단계 정의·초기 지급 재화 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialTable", menuName = "Abyss Chronicle/Tutorial Table")]
    public sealed class TutorialTable : ScriptableObject
    {
        public const int STEP_COUNT = 6;

        [SerializeField] private TutorialStepDefinition[] _steps = Array.Empty<TutorialStepDefinition>();
        [SerializeField] private TutorialStarterGrantDefinition _starterGrants = new();

        public IReadOnlyList<TutorialStepDefinition> Steps => _steps;
        public TutorialStarterGrantDefinition StarterGrants => _starterGrants;

        /// <summary>
        /// 단계 ID로 정의를 조회한다.
        /// </summary>
        public TutorialStepDefinition FindStep(TutorialStepId stepId)
        {
            foreach (var step in _steps)
            {
                if (step != null && step.StepId == stepId)
                    return step;
            }

            return null;
        }

        /// <summary>
        /// 순서상 다음 단계 정의를 반환한다.
        /// </summary>
        public TutorialStepDefinition FindNextStep(TutorialStepId stepId)
        {
            var nextId = (TutorialStepId)((int)stepId + 1);
            if ((int)nextId > STEP_COUNT)
                return null;

            return FindStep(nextId);
        }

        /// <summary>
        /// 첫 번째 단계 정의를 반환한다.
        /// </summary>
        public TutorialStepDefinition FindFirstStep()
        {
            return FindStep(TutorialStepId.FirstDispatch);
        }

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _steps = new[]
            {
                CreateStep(
                    TutorialStepId.FirstDispatch,
                    TutorialCompletionTrigger.DispatchStarted,
                    isGuidanceSkippable: false,
                    "tutorial.step.first_dispatch"),
                CreateStep(
                    TutorialStepId.FirstKillObservation,
                    TutorialCompletionTrigger.EnemyKilled,
                    isGuidanceSkippable: true,
                    "tutorial.step.first_kill"),
                CreateStep(
                    TutorialStepId.FirstEnhancement,
                    TutorialCompletionTrigger.EquipmentUpgraded,
                    isGuidanceSkippable: false,
                    "tutorial.step.first_enhance"),
                CreateStep(
                    TutorialStepId.FirstEquip,
                    TutorialCompletionTrigger.EquipmentEquipped,
                    isGuidanceSkippable: true,
                    "tutorial.step.first_equip"),
                CreateStep(
                    TutorialStepId.FirstSummon,
                    TutorialCompletionTrigger.SummonPerformed,
                    isGuidanceSkippable: false,
                    "tutorial.step.first_summon"),
                CreateStep(
                    TutorialStepId.FirstOfflineReward,
                    TutorialCompletionTrigger.OfflineRewardClaimed,
                    isGuidanceSkippable: true,
                    "tutorial.step.first_offline"),
            };

            _starterGrants = new TutorialStarterGrantDefinition
            {
                Gold = 150L,
                AbyssStone = 300L,
                SummonTicket = 0L,
            };
        }

        private static TutorialStepDefinition CreateStep(
            TutorialStepId stepId,
            TutorialCompletionTrigger trigger,
            bool isGuidanceSkippable,
            string displayNameKey)
        {
            return new TutorialStepDefinition
            {
                StepId = stepId,
                CompletionTrigger = trigger,
                IsGuidanceSkippable = isGuidanceSkippable,
                DisplayNameKey = displayNameKey,
            };
        }
    }

    /// <summary>
    /// 단일 튜토리얼 단계 정의.
    /// </summary>
    [Serializable]
    public sealed class TutorialStepDefinition
    {
        public TutorialStepId StepId;
        public TutorialCompletionTrigger CompletionTrigger;
        public bool IsGuidanceSkippable;
        public string DisplayNameKey;
    }

    /// <summary>
    /// FTUE 페이싱용 신규 유저 초기 재화 지급.
    /// </summary>
    [Serializable]
    public sealed class TutorialStarterGrantDefinition
    {
        public long Gold;
        public long AbyssStone;
        public long SummonTicket;
    }
}
