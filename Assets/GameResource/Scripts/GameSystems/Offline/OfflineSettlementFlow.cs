using Cysharp.Threading.Tasks;
using Backend.Object.Management;
using Backend.Object.UI.Offline;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 복귀 시 오프라인 정산과 요약 모달 표시를 조율한다. 스테이지 재생은 강제하지 않는다.
    /// </summary>
    public static class OfflineSettlementFlow
    {
        /// <summary>
        /// 복귀 시 정산을 시도하고, 보상이 있으면 요약 모달을 연다.
        /// </summary>
        public static async UniTask TrySettleOnReturnAsync()
        {
            var result = OfflineRuntimeProvider.Service.SettleOnReturn();
            if (!result.ShouldShowSummary)
                return;

            OfflineReturnSummaryContext.SetPending(result);
            await UIManager.OpenAsync<OfflineReturnSummaryPopup>();
        }
    }
}
