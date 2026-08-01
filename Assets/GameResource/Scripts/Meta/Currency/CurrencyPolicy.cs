namespace Backend.Meta.Currency
{
    /// <summary>
    /// 재화 분류. 하드→소프트 직접 구매 경로는 제공하지 않는다.
    /// </summary>
    public static class CurrencyPolicy
    {
        /// <summary>
        /// 소프트 재화 여부를 반환한다.
        /// </summary>
        public static bool IsSoftCurrency(CurrencyType type)
        {
            return type is CurrencyType.Gold or CurrencyType.ManaShard or CurrencyType.RelicFragment;
        }

        /// <summary>
        /// 하드 재화 여부를 반환한다.
        /// </summary>
        public static bool IsHardCurrency(CurrencyType type)
        {
            return type is CurrencyType.AbyssStone or CurrencyType.SummonTicket;
        }

        /// <summary>
        /// 메타 재화 여부를 반환한다.
        /// </summary>
        public static bool IsMetaCurrency(CurrencyType type)
        {
            return type is CurrencyType.LegacyPoint or CurrencyType.Reputation;
        }
    }
}
