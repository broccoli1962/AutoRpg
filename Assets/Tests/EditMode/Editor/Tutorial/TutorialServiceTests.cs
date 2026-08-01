using Backend.Meta.Achievements;
using Backend.Meta.Currency;
using Backend.Services.Analytics;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.Tutorial.Tests
{
    public class TutorialServiceTests
    {
        private TransactionLedger _ledger;
        private Wallet _wallet;
        private TutorialTable _table;
        private TutorialService _service;
        private TutorialEventBridge _bridge;
        private int _enteredCount;
        private int _completedCount;

        [SetUp]
        public void SetUp()
        {
            BackendAnalyticsEvents.ClearSubscribers();
            MetaGameplayEvents.ClearSubscribers();

            BackendAnalyticsEvents.TutorialStepEnteredReported += OnEntered;
            BackendAnalyticsEvents.TutorialStepCompletedReported += OnCompleted;

            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _table = ScriptableObject.CreateInstance<TutorialTable>();
            _table.ApplySpecDefaults();
            TutorialTableProvider.SetForTests(_table);

            _service = new TutorialService(_wallet);
            _service.Bootstrap(_table);
            _bridge = new TutorialEventBridge(_service, _table);
            _bridge.Subscribe();
        }

        [TearDown]
        public void TearDown()
        {
            _bridge?.Unsubscribe();
            BackendAnalyticsEvents.ClearSubscribers();
            MetaGameplayEvents.ClearSubscribers();
            TutorialTableProvider.ResetCache();

            if (_table != null)
                Object.DestroyImmediate(_table);
        }

        [Test]
        public void TutorialTable_DefinesSixStepsWithStarterGrants()
        {
            Assert.AreEqual(6, _table.Steps.Count);
            Assert.AreEqual(150L, _table.StarterGrants.Gold);
            Assert.AreEqual(300L, _table.StarterGrants.AbyssStone);
            Assert.IsFalse(_table.FindStep(TutorialStepId.FirstDispatch).IsGuidanceSkippable);
            Assert.IsTrue(_table.FindStep(TutorialStepId.FirstKillObservation).IsGuidanceSkippable);
        }

        [Test]
        public void Bootstrap_AppliesStarterGrantsOnce()
        {
            Assert.IsTrue(_service.InitialGrantsApplied);
            Assert.AreEqual(150L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(300L, _wallet.GetBalance(CurrencyType.AbyssStone));

            _service.Bootstrap(_table);
            Assert.AreEqual(150L, _wallet.GetBalance(CurrencyType.Gold));
        }

        [Test]
        public void EventBridge_AdvancesThroughAllSteps()
        {
            Assert.AreEqual(TutorialStepId.FirstDispatch, _service.CurrentStep);
            Assert.AreEqual(1, _enteredCount);

            MetaGameplayEvents.ReportDispatchStarted();
            Assert.AreEqual(TutorialStepId.FirstKillObservation, _service.CurrentStep);
            Assert.AreEqual(1, _completedCount);

            MetaGameplayEvents.ReportEnemyKills(1);
            MetaGameplayEvents.ReportEquipmentUpgrade();
            MetaGameplayEvents.ReportEquipmentEquipped();
            MetaGameplayEvents.ReportSummon();
            MetaGameplayEvents.ReportOfflineRewardClaimed();

            Assert.IsTrue(_service.IsTutorialComplete);
            Assert.IsFalse(_service.IsTutorialActive);
            Assert.AreEqual(TutorialStepId.None, _service.CurrentStep);
            Assert.AreEqual(6, _completedCount);
        }

        [Test]
        public void TrySkipCurrentGuidance_RespectsSkippableFlag()
        {
            MetaGameplayEvents.ReportDispatchStarted();

            Assert.IsFalse(_service.TrySkipCurrentGuidance(_table));

            MetaGameplayEvents.ReportEnemyKills(1);
            Assert.IsTrue(_service.CanSkipCurrentGuidance(_table));
            Assert.IsTrue(_service.TrySkipCurrentGuidance(_table));
            Assert.IsTrue(_service.IsCurrentGuidanceSkipped());
        }

        [Test]
        public void FromSaveData_RestoresProgressAndResumesCurrentStep()
        {
            MetaGameplayEvents.ReportDispatchStarted();
            MetaGameplayEvents.ReportEnemyKills(1);

            var saveData = _service.ToSaveData();
            var restored = TutorialService.FromSaveData(saveData, _wallet);
            restored.Bootstrap(_table);

            Assert.AreEqual(TutorialStepId.FirstEnhancement, restored.CurrentStep);
            Assert.IsTrue(restored.IsStepCompleted(TutorialStepId.FirstDispatch));
            Assert.IsTrue(restored.IsStepCompleted(TutorialStepId.FirstKillObservation));
            Assert.IsTrue(restored.InitialGrantsApplied);
        }

        [Test]
        public void TutorialServiceGate_BlocksWhileActive()
        {
            var gate = new TutorialServiceGate(_service);
            Assert.IsTrue(gate.IsTutorialActive);

            for (var i = 0; i < TutorialTable.STEP_COUNT; i++)
            {
                var step = _table.FindStep(_service.CurrentStep);
                if (step == null)
                    break;

                _service.TryHandleTrigger(step.CompletionTrigger, _table);
            }

            Assert.IsFalse(gate.IsTutorialActive);
        }

        private void OnEntered(int step)
        {
            _enteredCount++;
            Assert.Greater(step, 0);
        }

        private void OnCompleted(int step)
        {
            _completedCount++;
            Assert.Greater(step, 0);
        }
    }
}
