using System;
using UnityEngine;

namespace Backend.Simulation
{
    /// <summary>
    /// BalanceTable 기반 성장 곡선·진행 규칙 계산.
    /// </summary>
    public static class BalanceFormulas
    {
        private const float MIN_DENOMINATOR = 1e-6f;

        /// <summary>
        /// 통산 층 n(1-based)에서 구역 번호(1-based)를 반환한다.
        /// </summary>
        public static int GetZoneFromFloor(BalanceTable table, int floor)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (floor < 1)
                return 1;

            return (floor - 1) / table.FloorsPerZone + 1;
        }

        /// <summary>
        /// 통산 층 n의 몬스터 HP를 계산한다.
        /// </summary>
        public static double GetMonsterHp(BalanceTable table, int floor)
        {
            ValidateFloor(table, floor);
            var zoneMul = table.GetZoneMultiplier(GetZoneFromFloor(table, floor));
            return table.MonsterHpBase * PowGrowth(table.MonsterHpGrowth, floor - 1) * zoneMul;
        }

        /// <summary>
        /// 통산 층 n의 몬스터 ATK를 계산한다.
        /// </summary>
        public static double GetMonsterAtk(BalanceTable table, int floor)
        {
            ValidateFloor(table, floor);
            var zoneMul = table.GetZoneMultiplier(GetZoneFromFloor(table, floor));
            return table.MonsterAtkBase * PowGrowth(table.MonsterAtkGrowth, floor - 1) * zoneMul;
        }

        /// <summary>
        /// 통산 층 n의 몬스터 DEF를 계산한다.
        /// </summary>
        public static double GetMonsterDef(BalanceTable table, int floor)
        {
            ValidateFloor(table, floor);
            var zoneMul = table.GetZoneMultiplier(GetZoneFromFloor(table, floor));
            return table.MonsterDefBase * PowGrowth(table.MonsterDefGrowth, floor - 1) * zoneMul;
        }

        /// <summary>
        /// 통산 층 n의 골드 드롭량을 계산한다.
        /// </summary>
        public static double GetGoldDrop(BalanceTable table, int floor)
        {
            ValidateFloor(table, floor);
            return table.GoldDropBase * PowGrowth(table.GoldDropGrowth, floor - 1);
        }

        /// <summary>
        /// 장비 강화 레벨 L(0-based 시작)의 비용을 계산한다.
        /// </summary>
        public static double GetUpgradeCost(BalanceTable table, int level)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (level < 0)
                level = 0;

            return table.UpgradeCostBase * PowGrowth(table.UpgradeCostGrowth, level);
        }

        /// <summary>
        /// 최고 도달 층에 따른 장비 강화 레벨 상한 Lmax를 반환한다.
        /// </summary>
        public static int GetMaxUpgradeLevel(BalanceTable table, int highestFloorReached)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (highestFloorReached < 1)
                return 0;

            return Mathf.FloorToInt(table.UpgradeLevelCapMultiplier * highestFloorReached);
        }

        /// <summary>
        /// 캐릭터 레벨 L(1-based)에 필요한 경험치를 계산한다.
        /// </summary>
        public static double GetExpRequired(BalanceTable table, int level)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (level < 1)
                level = 1;

            return table.ExpRequiredBase * PowGrowth(table.ExpRequiredGrowth, level - 1);
        }

        /// <summary>
        /// 해당 층에서 허용된 최대 강화 레벨까지 1회 강화에 필요한 처치 수를 추정한다.
        /// </summary>
        public static double GetKillsPerUpgradeAtFloor(BalanceTable table, int floor)
        {
            ValidateFloor(table, floor);
            var lmax = GetMaxUpgradeLevel(table, floor);
            if (lmax <= 0)
                return double.PositiveInfinity;

            var upgradeLevel = Mathf.Max(0, lmax - 1);
            var cost = GetUpgradeCost(table, upgradeLevel);
            var goldDrop = GetGoldDrop(table, floor);
            return cost / Math.Max(goldDrop, MIN_DENOMINATOR);
        }

        /// <summary>
        /// 요구치(HP) 곡선이 보상(골드) 곡선보다 빠르게 성장하는지 층 구간에서 검증한다.
        /// </summary>
        public static bool IsRequirementCurveSteeperThanReward(BalanceTable table, int fromFloor, int toFloor)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (fromFloor < 1 || toFloor < fromFloor)
                return false;

            var startHp = GetMonsterHp(table, fromFloor);
            var endHp = GetMonsterHp(table, toFloor);
            var startGold = GetGoldDrop(table, fromFloor);
            var endGold = GetGoldDrop(table, toFloor);

            if (startHp <= 0d || startGold <= 0d)
                return false;

            var hpRatio = endHp / startHp;
            var goldRatio = endGold / startGold;
            return hpRatio > goldRatio;
        }

        private static void ValidateFloor(BalanceTable table, int floor)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (floor < 1)
                throw new ArgumentOutOfRangeException(nameof(floor), floor, "Floor must be >= 1.");
        }

        private static double PowGrowth(float growth, int exponent)
        {
            if (exponent <= 0)
                return 1d;

            return Math.Pow(growth, exponent);
        }
    }
}
