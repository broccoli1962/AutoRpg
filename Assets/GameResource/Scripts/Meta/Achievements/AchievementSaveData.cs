using System;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 업적 진행·수령 상태 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class AchievementSaveData
    {
        public AchievementProgressEntry[] ProgressEntries = Array.Empty<AchievementProgressEntry>();
        public string[] ClaimedTierIds = Array.Empty<string>();
    }

    /// <summary>
    /// 카테고리별 진행도 항목.
    /// </summary>
    [Serializable]
    public sealed class AchievementProgressEntry
    {
        public AchievementCategory Category;
        public long CurrentValue;
    }
}
