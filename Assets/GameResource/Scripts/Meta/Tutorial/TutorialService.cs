using System;
using System.Collections.Generic;
using Backend.Meta.Currency;
using Backend.Services.Analytics;

namespace Backend.Meta.Tutorial
{
    /// <summary>
    /// FTUE 튜토리얼 진행·세이브·분석·초기 지급을 담당한다.
    /// </summary>
    public sealed class TutorialService
    {
        private readonly Wallet _wallet;
        private readonly HashSet<int> _completedSteps = new();
        private readonly HashSet<int> _guidanceSkippedSteps = new();
        private readonly HashSet<int> _enteredThisSession = new();

        private TutorialStepId _currentStep = TutorialStepId.None;
        private bool _initialGrantsApplied;

        public TutorialService(Wallet wallet)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        /// <summary>
        /// 현재 활성 단계를 반환한다. 완료 시 None.
        /// </summary>
        public TutorialStepId CurrentStep => _currentStep;

        /// <summary>
        /// 튜토리얼이 아직 진행 중이면 true.
        /// </summary>
        public bool IsTutorialActive => _currentStep != TutorialStepId.None;

        /// <summary>
        /// 모든 단계를 완료했으면 true.
        /// </summary>
        public bool IsTutorialComplete =>
            _completedSteps.Count >= TutorialTable.STEP_COUNT;

        /// <summary>
        /// 초기 지급 재화가 적용되었는지 여부.
        /// </summary>
        public bool InitialGrantsApplied => _initialGrantsApplied;

        /// <summary>
        /// 단계 완료 여부를 반환한다.
        /// </summary>
        public bool IsStepCompleted(TutorialStepId stepId)
        {
            return stepId != TutorialStepId.None && _completedSteps.Contains((int)stepId);
        }

        /// <summary>
        /// 현재 단계 안내를 스킵할 수 있는지 여부.
        /// </summary>
        public bool CanSkipCurrentGuidance(TutorialTable table)
        {
            var definition = table?.FindStep(_currentStep);
            return definition != null && definition.IsGuidanceSkippable;
        }

        /// <summary>
        /// 현재 단계 안내가 스킵되었는지 여부.
        /// </summary>
        public bool IsCurrentGuidanceSkipped()
        {
            return _currentStep != TutorialStepId.None
                && _guidanceSkippedSteps.Contains((int)_currentStep);
        }

        /// <summary>
        /// 테이블 기준으로 튜토리얼 상태를 초기화하고 첫 단계에 진입한다.
        /// </summary>
        public void Bootstrap(TutorialTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (IsTutorialComplete)
            {
                _currentStep = TutorialStepId.None;
                return;
            }

            ApplyInitialGrantsInternal(table);

            if (_currentStep == TutorialStepId.None)
                _currentStep = ResolveFirstIncompleteStep(table);

            EnterCurrentStepInternal();
        }

        /// <summary>
        /// 현재 단계 안내를 스킵한다. 완료 트리거는 여전히 필요하다.
        /// </summary>
        public bool TrySkipCurrentGuidance(TutorialTable table)
        {
            if (!CanSkipCurrentGuidance(table))
                return false;

            _guidanceSkippedSteps.Add((int)_currentStep);
            return true;
        }

        /// <summary>
        /// 게임플레이 트리거에 따라 현재 단계 완료를 시도한다.
        /// </summary>
        public bool TryHandleTrigger(TutorialCompletionTrigger trigger, TutorialTable table)
        {
            if (!IsTutorialActive || table == null)
                return false;

            var definition = table.FindStep(_currentStep);
            if (definition == null || definition.CompletionTrigger != trigger)
                return false;

            CompleteCurrentStepInternal(table);
            return true;
        }

        /// <summary>
        /// 세션 재진입 시 현재 단계 진입 분석을 다시 발행한다.
        /// </summary>
        public void ResumeSession(TutorialTable table)
        {
            if (table == null || !IsTutorialActive)
                return;

            EnterCurrentStepInternal();
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public TutorialSaveData ToSaveData()
        {
            var completed = new int[_completedSteps.Count];
            _completedSteps.CopyTo(completed);

            var skipped = new int[_guidanceSkippedSteps.Count];
            _guidanceSkippedSteps.CopyTo(skipped);

            return new TutorialSaveData
            {
                CurrentStep = (int)_currentStep,
                CompletedStepIds = completed,
                InitialGrantsApplied = _initialGrantsApplied,
                GuidanceSkippedStepIds = skipped,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 TutorialService 를 복원한다.
        /// </summary>
        public static TutorialService FromSaveData(TutorialSaveData saveData, Wallet wallet)
        {
            var service = new TutorialService(wallet);

            if (saveData == null)
                return service;

            service._currentStep = (TutorialStepId)Math.Max(0, saveData.CurrentStep);
            service._initialGrantsApplied = saveData.InitialGrantsApplied;

            if (saveData.CompletedStepIds != null)
            {
                foreach (var stepId in saveData.CompletedStepIds)
                {
                    if (stepId > 0)
                        service._completedSteps.Add(stepId);
                }
            }

            if (saveData.GuidanceSkippedStepIds != null)
            {
                foreach (var stepId in saveData.GuidanceSkippedStepIds)
                {
                    if (stepId > 0)
                        service._guidanceSkippedSteps.Add(stepId);
                }
            }

            if (service._completedSteps.Count >= TutorialTable.STEP_COUNT)
                service._currentStep = TutorialStepId.None;

            return service;
        }

        private void ApplyInitialGrantsInternal(TutorialTable table)
        {
            if (_initialGrantsApplied)
                return;

            var grants = table.StarterGrants;
            if (grants == null)
                return;

            if (grants.Gold > 0L)
            {
                _wallet.TryCredit(
                    CurrencyType.Gold,
                    grants.Gold,
                    CurrencyReasonCodes.TutorialStarterGrant);
            }

            if (grants.AbyssStone > 0L)
            {
                _wallet.TryCredit(
                    CurrencyType.AbyssStone,
                    grants.AbyssStone,
                    CurrencyReasonCodes.TutorialStarterGrant);
            }

            if (grants.SummonTicket > 0L)
            {
                _wallet.TryCredit(
                    CurrencyType.SummonTicket,
                    grants.SummonTicket,
                    CurrencyReasonCodes.TutorialStarterGrant);
            }

            _initialGrantsApplied = true;
        }

        private TutorialStepId ResolveFirstIncompleteStep(TutorialTable table)
        {
            for (var stepIndex = 1; stepIndex <= TutorialTable.STEP_COUNT; stepIndex++)
            {
                var stepId = (TutorialStepId)stepIndex;
                if (!IsStepCompleted(stepId))
                    return stepId;
            }

            return TutorialStepId.None;
        }

        private void CompleteCurrentStepInternal(TutorialTable table)
        {
            if (_currentStep == TutorialStepId.None)
                return;

            var completedStep = _currentStep;
            _completedSteps.Add((int)completedStep);
            BackendAnalyticsEvents.ReportTutorialStepCompleted((int)completedStep);

            var next = table.FindNextStep(completedStep);
            _currentStep = next != null ? next.StepId : TutorialStepId.None;

            if (_currentStep != TutorialStepId.None)
                EnterCurrentStepInternal();
        }

        private void EnterCurrentStepInternal()
        {
            if (_currentStep == TutorialStepId.None)
                return;

            var stepIndex = (int)_currentStep;
            if (_enteredThisSession.Contains(stepIndex))
                return;

            _enteredThisSession.Add(stepIndex);
            BackendAnalyticsEvents.ReportTutorialStepEntered(stepIndex);
        }
    }
}
