namespace Backend.Meta.Currency
{
    /// <summary>
    /// 재화 증감 연산 결과. 차감 실패 등은 예외 대신 이 객체로 반환한다.
    /// </summary>
    public readonly struct CurrencyTransactionResult
    {
        public bool Success { get; }
        public CurrencyType CurrencyType { get; }
        public long Delta { get; }
        public long BalanceAfter { get; }
        public string ReasonCode { get; }
        public string FailureReason { get; }

        private CurrencyTransactionResult(
            bool success,
            CurrencyType currencyType,
            long delta,
            long balanceAfter,
            string reasonCode,
            string failureReason)
        {
            Success = success;
            CurrencyType = currencyType;
            Delta = delta;
            BalanceAfter = balanceAfter;
            ReasonCode = reasonCode;
            FailureReason = failureReason;
        }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static CurrencyTransactionResult Succeeded(
            CurrencyType currencyType,
            long delta,
            long balanceAfter,
            string reasonCode)
        {
            return new CurrencyTransactionResult(
                true,
                currencyType,
                delta,
                balanceAfter,
                reasonCode,
                null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static CurrencyTransactionResult Failed(
            CurrencyType currencyType,
            long requestedDelta,
            long balanceAfter,
            string reasonCode,
            string failureReason)
        {
            return new CurrencyTransactionResult(
                false,
                currencyType,
                requestedDelta,
                balanceAfter,
                reasonCode,
                failureReason);
        }
    }
}
