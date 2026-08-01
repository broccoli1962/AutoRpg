using System;
using Backend.Meta.Characters;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 소환 원장 1회분 개별 추첨 기록.
    /// </summary>
    [Serializable]
    public struct GachaPullRecord
    {
        public ExplorerGrade Grade;
        public string CharacterId;
        public bool TriggeredSsrPity;
        public bool TriggeredUrPity;
        public bool TenPullGuaranteeApplied;
    }
}
