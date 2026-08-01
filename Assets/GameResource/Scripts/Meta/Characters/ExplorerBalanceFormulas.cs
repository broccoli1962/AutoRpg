using System;
using Backend.Simulation;

namespace Backend.Meta.Characters
{
    /// <summary>
    /// BalanceTable 기반 탐험가 등급·한계돌파 수치 계산.
    /// </summary>
    public static class ExplorerBalanceFormulas
    {
        private const string INVALID_TABLE = "BalanceTable is null.";
        private const string INVALID_GRADE = "Invalid explorer grade.";

        /// <summary>
        /// 한계돌파 최대 단계를 반환한다.
        /// </summary>
        public static int GetMaxLimitBreakStage(BalanceTable table)
        {
            ValidateTable(table);
            return table.MaxLimitBreakStage;
        }

        /// <summary>
        /// 등급 기본 스탯 배율을 반환한다.
        /// </summary>
        public static float GetBaseStatMultiplier(BalanceTable table, ExplorerGrade grade)
        {
            ValidateGrade(table, grade, out var gradeIndex);
            return table.GetGradeStatMultiplier(gradeIndex);
        }

        /// <summary>
        /// 한계돌파 단계를 반영한 최종 스탯 배율을 반환한다.
        /// </summary>
        public static float GetStatMultiplier(
            BalanceTable table,
            ExplorerGrade grade,
            int limitBreakStage)
        {
            ValidateGrade(table, grade, out _);
            var clampedStage = ClampLimitBreakStage(table, limitBreakStage);
            var baseMultiplier = GetBaseStatMultiplier(table, grade);
            return baseMultiplier + table.LimitBreakStatBonusPerStage * clampedStage;
        }

        /// <summary>
        /// 중복 획득 시 지급되는 조각 수를 반환한다.
        /// </summary>
        public static int GetDuplicateFragmentYield(BalanceTable table, ExplorerGrade grade)
        {
            ValidateGrade(table, grade, out var gradeIndex);
            return table.GetDuplicateFragmentYield(gradeIndex);
        }

        /// <summary>
        /// 다음 한계돌파 단계에 필요한 조각 수를 반환한다.
        /// </summary>
        public static int GetLimitBreakFragmentCost(
            BalanceTable table,
            ExplorerGrade grade,
            int currentStage)
        {
            ValidateGrade(table, grade, out var gradeIndex);

            if (currentStage >= table.MaxLimitBreakStage)
                return 0;

            return table.GetLimitBreakFragmentCost(gradeIndex, currentStage + 1);
        }

        /// <summary>
        /// 한계돌파 단계를 반영한 스킬 레벨 상한을 반환한다.
        /// </summary>
        public static int GetSkillLevelCap(
            BalanceTable table,
            ExplorerGrade grade,
            int limitBreakStage)
        {
            ValidateGrade(table, grade, out _);
            var clampedStage = ClampLimitBreakStage(table, limitBreakStage);
            return table.BaseSkillLevelCap
                + table.LimitBreakSkillCapBonusPerStage * clampedStage;
        }

        /// <summary>
        /// 한계돌파가 가능한지 판정한다.
        /// </summary>
        public static bool CanLimitBreak(
            BalanceTable table,
            ExplorerGrade grade,
            int currentStage,
            int fragmentCount)
        {
            if (table == null || currentStage >= table.MaxLimitBreakStage)
                return false;

            var cost = GetLimitBreakFragmentCost(table, grade, currentStage);
            return fragmentCount >= cost && cost > 0;
        }

        private static int ClampLimitBreakStage(BalanceTable table, int limitBreakStage)
        {
            if (limitBreakStage < 0)
                return 0;

            return Math.Min(limitBreakStage, table.MaxLimitBreakStage);
        }

        private static void ValidateTable(BalanceTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table), INVALID_TABLE);
        }

        private static void ValidateGrade(
            BalanceTable table,
            ExplorerGrade grade,
            out int gradeIndex)
        {
            ValidateTable(table);
            gradeIndex = (int)grade;

            if (gradeIndex < 0 || gradeIndex > (int)ExplorerGrade.UR)
                throw new ArgumentOutOfRangeException(nameof(grade), grade, INVALID_GRADE);
        }
    }
}
