using System.Collections.Generic;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 단차·10연차 소환 세션 결과.
    /// </summary>
    public sealed class GachaSummonResult
    {
        private const string INSUFFICIENT_BALANCE = "Insufficient balance for summon.";
        private const string INVALID_TABLE = "Gacha rate table is invalid.";

        public bool Success { get; }
        public string FailureReason { get; }
        public long Seed { get; }
        public string BannerId { get; }
        public IReadOnlyList<GachaPullResult> Pulls { get; }
        public int SsrCounterAfter { get; }
        public int UrCounterAfter { get; }

        private GachaSummonResult(
            bool success,
            string failureReason,
            long seed,
            string bannerId,
            IReadOnlyList<GachaPullResult> pulls,
            int ssrCounterAfter,
            int urCounterAfter)
        {
            Success = success;
            FailureReason = failureReason;
            Seed = seed;
            BannerId = bannerId;
            Pulls = pulls ?? System.Array.Empty<GachaPullResult>();
            SsrCounterAfter = ssrCounterAfter;
            UrCounterAfter = urCounterAfter;
        }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static GachaSummonResult Succeeded(
            long seed,
            string bannerId,
            IReadOnlyList<GachaPullResult> pulls,
            GachaPityState pity)
        {
            return new GachaSummonResult(
                true,
                null,
                seed,
                bannerId,
                pulls,
                pity.GetSsrCounter(),
                pity.GetUrCounter());
        }

        /// <summary>
        /// 잔액 부족 실패 결과를 생성한다.
        /// </summary>
        public static GachaSummonResult InsufficientBalance()
        {
            return new GachaSummonResult(
                false,
                INSUFFICIENT_BALANCE,
                0L,
                null,
                null,
                0,
                0);
        }

        /// <summary>
        /// 확률 테이블 검증 실패 결과를 생성한다.
        /// </summary>
        public static GachaSummonResult InvalidTable()
        {
            return new GachaSummonResult(
                false,
                INVALID_TABLE,
                0L,
                null,
                null,
                0,
                0);
        }
    }
}
