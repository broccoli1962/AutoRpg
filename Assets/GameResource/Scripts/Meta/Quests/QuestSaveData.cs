using System;

namespace Backend.Meta.Quests
{
    /// <summary>
    /// 퀘스트 진행·수령 상태 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class QuestSaveData
    {
        public int DailyPeriodKey;
        public int WeeklyPeriodKey;
        public QuestProgressEntry[] ProgressEntries = Array.Empty<QuestProgressEntry>();
        public string[] ClaimedQuestIds = Array.Empty<string>();
        public bool DailyCompletionChestClaimed;
        public bool WeeklyCompletionChestClaimed;
    }

    /// <summary>
    /// 퀘스트별 진행도 항목.
    /// </summary>
    [Serializable]
    public sealed class QuestProgressEntry
    {
        public string QuestId;
        public int CurrentCount;
    }
}
