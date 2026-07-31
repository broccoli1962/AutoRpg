using NUnit.Framework;
using UnityEngine;

namespace Backend.Simulation.Editor.Tests
{
    public class BalanceFormulasTests
    {
        private BalanceTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = ScriptableObject.CreateInstance<BalanceTable>();
            _table.ApplySpecDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            if (_table != null)
                UnityEngine.Object.DestroyImmediate(_table);
        }

        [Test]
        public void Damage_IsPositive_ForWideStatCombinations()
        {
            var attacks = new[] { 1f, 10f, 100f, 10_000f, 1_000_000f };
            var defenses = new[] { 0f, 1f, 10f, 100f, 10_000f, 1_000_000f };

            foreach (var atk in attacks)
            {
                foreach (var def in defenses)
                {
                    var input = new DamageInput(atk, def, 0.25f, 1.5f, 1.2f, 1f);
                    var damage = DamageCalculator.CalculateDamage(input);
                    Assert.Greater(damage, 0f, $"ATK={atk}, DEF={def}");
                }
            }
        }

        [Test]
        public void Damage_IsPositive_WhenDenominatorWouldBeZero()
        {
            var input = new DamageInput(0f, 0f, 0f, 1f, 1f, 1f);
            var damage = DamageCalculator.CalculateDamage(input);
            Assert.Greater(damage, 0f);
        }

        [Test]
        public void Damage_UsesDampedFormula_NotLinearSubtraction()
        {
            var damped = DamageCalculator.CalculateDamage(new DamageInput(100f, 200f, 0f, 1f, 1f, 1f));
            var linearLegacy = 100f - 200f * 0.5f;
            Assert.Greater(damped, linearLegacy);
            Assert.AreEqual(100f * 100f / 300f, damped, 0.001f);
        }

        [Test]
        public void MonsterHp_Floor1_MatchesCurveFormula()
        {
            Assert.AreEqual(100d, BalanceFormulas.GetMonsterHp(_table, 1), 1e-6);
        }

        [Test]
        public void MonsterHp_Floor50_MatchesCurveFormula()
        {
            var zoneMul = _table.GetZoneMultiplier(BalanceFormulas.GetZoneFromFloor(_table, 50));
            var expected = 100d * System.Math.Pow(_table.MonsterHpGrowth, 49) * zoneMul;
            Assert.AreEqual(expected, BalanceFormulas.GetMonsterHp(_table, 50), 1e-3);
        }

        [Test]
        public void MonsterHp_Floor200_MatchesCurveFormula()
        {
            var zoneMul = _table.GetZoneMultiplier(BalanceFormulas.GetZoneFromFloor(_table, 200));
            var expected = 100d * System.Math.Pow(_table.MonsterHpGrowth, 199) * zoneMul;
            Assert.AreEqual(expected, BalanceFormulas.GetMonsterHp(_table, 200), expected * 1e-9 + 1e-3);
        }

        [Test]
        public void RequirementCurve_GrowsFasterThanRewardCurve()
        {
            Assert.IsTrue(BalanceFormulas.IsRequirementCurveSteeperThanReward(_table, 1, 200));

            for (var floor = 2; floor <= 200; floor++)
            {
                var hp = BalanceFormulas.GetMonsterHp(_table, floor);
                var prevHp = BalanceFormulas.GetMonsterHp(_table, floor - 1);
                var gold = BalanceFormulas.GetGoldDrop(_table, floor);
                var prevGold = BalanceFormulas.GetGoldDrop(_table, floor - 1);

                Assert.Greater(hp / prevHp, gold / prevGold, $"Floor {floor} should have steeper HP growth.");
            }
        }

        [Test]
        public void MaxUpgradeLevel_FollowsFloorCapRule()
        {
            Assert.AreEqual(1, BalanceFormulas.GetMaxUpgradeLevel(_table, 1));
            Assert.AreEqual(75, BalanceFormulas.GetMaxUpgradeLevel(_table, 50));
            Assert.AreEqual(300, BalanceFormulas.GetMaxUpgradeLevel(_table, 200));
        }

        [Test]
        public void KillsPerUpgrade_StaysWithinFiveToTen_ForFloorsOneToTwoHundred()
        {
            for (var floor = 1; floor <= 200; floor++)
            {
                var kills = BalanceFormulas.GetKillsPerUpgradeAtFloor(_table, floor);
                Assert.GreaterOrEqual(kills, 5d, $"Floor {floor} kills below 5.");
                Assert.LessOrEqual(kills, 10d, $"Floor {floor} kills above 10.");
            }
        }

        [Test]
        public void ZoneMultiplier_MatchesSpecTable()
        {
            Assert.AreEqual(1.00f, _table.GetZoneMultiplier(1), 0.001f);
            Assert.AreEqual(1.10f, _table.GetZoneMultiplier(2), 0.001f);
            Assert.AreEqual(2.75f, _table.GetZoneMultiplier(8), 0.001f);
            Assert.AreEqual(2.75f * Mathf.Pow(1.15f, 1), _table.GetZoneMultiplier(9), 0.001f);
        }
    }
}
