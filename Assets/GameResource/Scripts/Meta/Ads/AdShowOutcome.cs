namespace Backend.Meta.Ads
{
    /// <summary>
    /// 광고 표시 결과.
    /// </summary>
    public enum AdShowOutcome
    {
        Completed = 0,
        Failed = 1,
        NotLoaded = 2,
        Skipped = 3,
        Blocked = 4,
    }
}
