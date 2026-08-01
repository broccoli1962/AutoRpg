namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// UI 팝업에 전달할 최근 오프라인 정산 결과 보관.
    /// </summary>
    public static class OfflineReturnSummaryContext
    {
        private static OfflineSettlementResult _pending;

        /// <summary>
        /// 표시 대기 중인 정산 결과.
        /// </summary>
        public static OfflineSettlementResult Pending => _pending;

        /// <summary>
        /// 정산 결과를 팝업 표시용으로 등록한다.
        /// </summary>
        public static void SetPending(OfflineSettlementResult result)
        {
            _pending = result;
        }

        /// <summary>
        /// 보관된 결과를 비운다.
        /// </summary>
        public static void Clear()
        {
            _pending = null;
        }
    }
}
