using System;
using Backend.Chronicle;
using Backend.Meta.Characters;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 확률·천장 기반 등급 추첨 순수 로직.
    /// </summary>
    public static class GachaRoller
    {
        /// <summary>
        /// 천장·확률 테이블에 따라 등급 1회를 추첨한다. 호출 전 카운터 증가는 GachaService 가 담당한다.
        /// </summary>
        public static ExplorerGrade RollGrade(
            GachaRateTable table,
            GachaPityState pity,
            IRandomSource random,
            out bool triggeredSsrPity,
            out bool triggeredUrPity)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (pity == null)
                throw new ArgumentNullException(nameof(pity));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            triggeredSsrPity = false;
            triggeredUrPity = false;

            if (pity.UrCounter >= table.UrPityThreshold)
            {
                triggeredUrPity = true;
                return ExplorerGrade.UR;
            }

            if (pity.SsrCounter >= table.SsrPityThreshold)
            {
                triggeredSsrPity = true;
                return RollSsrPlusGrade(table, random);
            }

            return RollNormalGrade(table, random);
        }

        /// <summary>
        /// 획득 등급에 따라 천장 카운터를 갱신한다.
        /// </summary>
        public static void ApplyPityReset(GachaPityState pity, ExplorerGrade grade)
        {
            if (grade >= ExplorerGrade.SSR)
                pity.ResetSsrCounter();

            if (grade == ExplorerGrade.UR)
                pity.ResetUrCounter();
        }

        /// <summary>
        /// 10연차 SR 이상 보장을 적용한다. SR 이상이 없으면 마지막 슬롯을 SR 로 교체한다.
        /// </summary>
        public static GachaPullResult ApplyTenPullGuarantee(
            GachaPullResult[] pulls,
            IGachaCharacterPool pool,
            IRandomSource random)
        {
            if (pulls == null || pulls.Length == 0)
                throw new ArgumentException("Pull list must not be empty.", nameof(pulls));

            for (var i = 0; i < pulls.Length; i++)
            {
                if (pulls[i].Grade >= ExplorerGrade.SR)
                    return pulls[^1];
            }

            var lastIndex = pulls.Length - 1;
            var last = pulls[lastIndex];
            var characterId = pool.PickCharacter(ExplorerGrade.SR, random);

            var guaranteed = new GachaPullResult(
                ExplorerGrade.SR,
                characterId,
                last.TriggeredSsrPity,
                last.TriggeredUrPity,
                tenPullGuaranteeApplied: true);

            pulls[lastIndex] = guaranteed;
            return guaranteed;
        }

        private static ExplorerGrade RollNormalGrade(GachaRateTable table, IRandomSource random)
        {
            var roll = random.NextInt(0, 10_000);

            if (roll < table.RateR)
                return ExplorerGrade.R;

            roll -= table.RateR;
            if (roll < table.RateSr)
                return ExplorerGrade.SR;

            roll -= table.RateSr;
            if (roll < table.RateSsr)
                return ExplorerGrade.SSR;

            return ExplorerGrade.UR;
        }

        private static ExplorerGrade RollSsrPlusGrade(GachaRateTable table, IRandomSource random)
        {
            var ssrPlusTotal = table.RateSsr + table.RateUr;
            if (ssrPlusTotal <= 0)
                return ExplorerGrade.SSR;

            var roll = random.NextInt(0, ssrPlusTotal);
            return roll < table.RateSsr ? ExplorerGrade.SSR : ExplorerGrade.UR;
        }
    }
}
