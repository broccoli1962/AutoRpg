using System;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 오프라인 진행·정산 시각 세이브 데이터.
    /// </summary>
    [Serializable]
    public sealed class OfflineProgressSaveData
    {
        public long LastSettlementUtcTicks;
        public int CurrentFloor = 1;
        public int InnFacilityLevel;
        public bool HasActiveMonthlyContract;
    }
}
