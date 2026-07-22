namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 런타임 로그 피드 필터 탭 (12_UIUX.md).
    /// </summary>
    public enum LogFeedFilter
    {
        All,
        Combat,
        Discovery,
        Event,
        Narrative
    }

    public static class LogFeedFilterUtil
    {
        public static string GetDisplayLabel(LogFeedFilter filter)
        {
            return filter switch
            {
                LogFeedFilter.All => "전체",
                LogFeedFilter.Combat => "전투",
                LogFeedFilter.Discovery => "발견",
                LogFeedFilter.Event => "이벤트",
                LogFeedFilter.Narrative => "서사",
                _ => filter.ToString()
            };
        }

        public static LogFeedFilter Cycle(LogFeedFilter current)
        {
            return current switch
            {
                LogFeedFilter.All => LogFeedFilter.Combat,
                LogFeedFilter.Combat => LogFeedFilter.Discovery,
                LogFeedFilter.Discovery => LogFeedFilter.Event,
                LogFeedFilter.Event => LogFeedFilter.Narrative,
                _ => LogFeedFilter.All
            };
        }

        public static bool Matches(LogFeedFilter filter, LogFeedFilter group)
        {
            return filter == LogFeedFilter.All || filter == group;
        }
    }
}
