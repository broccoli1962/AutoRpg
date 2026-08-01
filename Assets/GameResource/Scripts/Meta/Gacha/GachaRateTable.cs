using System;
using Backend.Meta.Characters;
using UnityEngine;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// docs/spec.md 4.2 소환 확률·비용·천장 임계값을 담는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "GachaRateTable", menuName = "Abyss Chronicle/Gacha Rate Table")]
    public sealed class GachaRateTable : ScriptableObject
    {
        private const int BASIS_POINTS_TOTAL = 10_000;

        [Header("Grade rates (basis points, sum must equal 10_000)")]
        [SerializeField] private int _rateR = 7_000;
        [SerializeField] private int _rateSr = 2_400;
        [SerializeField] private int _rateSsr = 550;
        [SerializeField] private int _rateUr = 50;

        [Header("Costs (Abyss Stone)")]
        [SerializeField] private int _singlePullCost = 300;
        [SerializeField] private int _tenPullCost = 2_700;

        [Header("Pity thresholds")]
        [SerializeField] private int _ssrPityThreshold = 100;
        [SerializeField] private int _urPityThreshold = 200;

        public int RateR => _rateR;
        public int RateSr => _rateSr;
        public int RateSsr => _rateSsr;
        public int RateUr => _rateUr;
        public int SinglePullCost => _singlePullCost;
        public int TenPullCost => _tenPullCost;
        public int SsrPityThreshold => _ssrPityThreshold;
        public int UrPityThreshold => _urPityThreshold;

        /// <summary>
        /// 등급별 확률(만분율)을 반환한다.
        /// </summary>
        public int GetRateBasisPoints(ExplorerGrade grade)
        {
            return grade switch
            {
                ExplorerGrade.R => _rateR,
                ExplorerGrade.SR => _rateSr,
                ExplorerGrade.SSR => _rateSsr,
                ExplorerGrade.UR => _rateUr,
                _ => 0,
            };
        }

        /// <summary>
        /// spec.md 4.2 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _rateR = 7_000;
            _rateSr = 2_400;
            _rateSsr = 550;
            _rateUr = 50;
            _singlePullCost = 300;
            _tenPullCost = 2_700;
            _ssrPityThreshold = 100;
            _urPityThreshold = 200;
        }

        /// <summary>
        /// 확률 합계가 정확히 100%(10_000 bp)인지 검증한다.
        /// </summary>
        public void ValidateRates()
        {
            var total = _rateR + _rateSr + _rateSsr + _rateUr;
            if (total != BASIS_POINTS_TOTAL)
            {
                throw new InvalidOperationException(
                    $"[GachaRateTable] Grade rates must sum to {BASIS_POINTS_TOTAL} bp (100%), but got {total}.");
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            try
            {
                ValidateRates();
            }
            catch (InvalidOperationException)
            {
                // 런타임 로드 시에만 검증 실패를 표면화한다.
            }
        }
    }
}
