using Backend.Meta.Characters;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 소환 1회 결과.
    /// </summary>
    public readonly struct GachaPullResult
    {
        public ExplorerGrade Grade { get; }
        public string CharacterId { get; }
        public bool TriggeredSsrPity { get; }
        public bool TriggeredUrPity { get; }
        public bool TenPullGuaranteeApplied { get; }

        public GachaPullResult(
            ExplorerGrade grade,
            string characterId,
            bool triggeredSsrPity = false,
            bool triggeredUrPity = false,
            bool tenPullGuaranteeApplied = false)
        {
            Grade = grade;
            CharacterId = characterId;
            TriggeredSsrPity = triggeredSsrPity;
            TriggeredUrPity = triggeredUrPity;
            TenPullGuaranteeApplied = tenPullGuaranteeApplied;
        }
    }
}
