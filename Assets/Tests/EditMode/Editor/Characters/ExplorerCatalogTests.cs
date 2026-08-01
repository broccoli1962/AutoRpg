using System;
using NUnit.Framework;
using Backend.Meta.Characters;
using Backend.Simulation;
using UnityEngine;

namespace Backend.Meta.Characters.Tests
{
    public class ExplorerCatalogTests
    {
        private BalanceTable _table;
        private ExplorerCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _table = ScriptableObject.CreateInstance<BalanceTable>();
            _table.ApplySpecDefaults();
            _catalog = new ExplorerCatalog();
        }

        [TearDown]
        public void TearDown()
        {
            if (_table != null)
                UnityEngine.Object.DestroyImmediate(_table);
        }

        [Test]
        public void ExplorerGrade_DefinesFourGrades()
        {
            var values = Enum.GetValues(typeof(ExplorerGrade));
            Assert.AreEqual(4, values.Length);
            CollectionAssert.AreEqual(
                new[] { ExplorerGrade.R, ExplorerGrade.SR, ExplorerGrade.SSR, ExplorerGrade.UR },
                values);
        }

        [Test]
        public void BalanceTable_DefinesGradeStatMultipliers()
        {
            Assert.AreEqual(1.00f, _table.GetGradeStatMultiplier(0), 0.001f);
            Assert.AreEqual(1.15f, _table.GetGradeStatMultiplier(1), 0.001f);
            Assert.AreEqual(1.35f, _table.GetGradeStatMultiplier(2), 0.001f);
            Assert.AreEqual(1.60f, _table.GetGradeStatMultiplier(3), 0.001f);
        }

        [Test]
        public void TryAcquire_GrantsNewCharacter_WhenNotOwned()
        {
            var result = _catalog.TryAcquire("explorer_001", ExplorerGrade.SR, _table);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.IsNewCharacter);
            Assert.AreEqual(0, result.FragmentsGranted);
            Assert.IsTrue(_catalog.IsOwned("explorer_001"));
            Assert.IsTrue(_catalog.IsInCompendium("explorer_001"));
            Assert.AreEqual(ExplorerGrade.SR, _catalog.GetOwned("explorer_001").Grade);
        }

        [TestCase(ExplorerGrade.R, 10)]
        [TestCase(ExplorerGrade.SR, 20)]
        [TestCase(ExplorerGrade.SSR, 50)]
        [TestCase(ExplorerGrade.UR, 100)]
        public void TryAcquire_ConvertsDuplicateToGradeSpecificFragments(
            ExplorerGrade grade,
            int expectedFragments)
        {
            _catalog.TryAcquire("explorer_dup", grade, _table);

            var result = _catalog.TryAcquire("explorer_dup", grade, _table);

            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.IsNewCharacter);
            Assert.AreEqual(expectedFragments, result.FragmentsGranted);
            Assert.AreEqual(expectedFragments, _catalog.GetFragmentCount("explorer_dup"));
        }

        [Test]
        public void TryLimitBreak_IncreasesStage_AndSpendsFragments()
        {
            _catalog.TryAcquire("explorer_lb", ExplorerGrade.SSR, _table);
            _catalog.TryAcquire("explorer_lb", ExplorerGrade.SSR, _table);
            _catalog.TryAcquire("explorer_lb", ExplorerGrade.SSR, _table);

            var beforeFragments = _catalog.GetFragmentCount("explorer_lb");
            var result = _catalog.TryLimitBreak("explorer_lb", _table);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.PreviousStage);
            Assert.AreEqual(1, result.NewStage);
            Assert.AreEqual(50, result.FragmentsSpent);
            Assert.AreEqual(beforeFragments - 50, _catalog.GetFragmentCount("explorer_lb"));
        }

        [Test]
        public void TryLimitBreak_DoesNotExceedMaxStage()
        {
            _catalog.TryAcquire("explorer_max", ExplorerGrade.R, _table);

            for (var i = 0; i < 20; i++)
                _catalog.TryAcquire("explorer_max", ExplorerGrade.R, _table);

            for (var stage = 1; stage <= _table.MaxLimitBreakStage; stage++)
            {
                var result = _catalog.TryLimitBreak("explorer_max", _table);
                Assert.IsTrue(result.Success, $"Stage {stage} should succeed.");
                Assert.AreEqual(stage, result.NewStage);
            }

            Assert.AreEqual(_table.MaxLimitBreakStage, _catalog.GetLimitBreakStage("explorer_max"));

            var blocked = _catalog.TryLimitBreak("explorer_max", _table);
            Assert.IsFalse(blocked.Success);
            Assert.AreEqual(_table.MaxLimitBreakStage, blocked.NewStage);
        }

        [Test]
        public void GetStatMultiplier_IncludesLimitBreakBonus()
        {
            var baseMultiplier = ExplorerBalanceFormulas.GetBaseStatMultiplier(
                _table,
                ExplorerGrade.UR);
            var atStage3 = ExplorerBalanceFormulas.GetStatMultiplier(
                _table,
                ExplorerGrade.UR,
                3);

            Assert.AreEqual(
                baseMultiplier + _table.LimitBreakStatBonusPerStage * 3f,
                atStage3,
                0.0001f);
        }

        [Test]
        public void GetSkillLevelCap_IncreasesWithLimitBreak()
        {
            var cap0 = ExplorerBalanceFormulas.GetSkillLevelCap(_table, ExplorerGrade.SR, 0);
            var cap5 = ExplorerBalanceFormulas.GetSkillLevelCap(_table, ExplorerGrade.SR, 5);

            Assert.AreEqual(_table.BaseSkillLevelCap, cap0);
            Assert.AreEqual(
                _table.BaseSkillLevelCap + _table.LimitBreakSkillCapBonusPerStage * 5,
                cap5);
        }

        [Test]
        public void SaveAndLoad_PreservesOwnedAndCompendium()
        {
            _catalog.TryAcquire("explorer_a", ExplorerGrade.R, _table);
            _catalog.TryAcquire("explorer_a", ExplorerGrade.R, _table);
            _catalog.TryAcquire("explorer_a", ExplorerGrade.R, _table);
            _catalog.TryAcquire("explorer_b", ExplorerGrade.UR, _table);
            _catalog.TryLimitBreak("explorer_a", _table);

            var restored = ExplorerCatalog.FromSaveData(_catalog.ToSaveData());

            Assert.AreEqual(2, restored.OwnedCount);
            Assert.AreEqual(2, restored.CompendiumCount);
            Assert.AreEqual(1, restored.GetLimitBreakStage("explorer_a"));
            Assert.AreEqual(ExplorerGrade.UR, restored.GetOwned("explorer_b").Grade);
        }
    }
}
