using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// LLM 스트리밍 로그 Typewriter 표시용 헬퍼.
    /// </summary>
    public static class LogTypewriterHelper
    {
        public const int CharDelayMs = 18;

        public static LogEntry WithPartialText(LogEntry source, string partialText, bool isPending)
        {
            if (source == null)
                return null;

            return new LogEntry
            {
                EventId = source.EventId,
                EventType = source.EventType,
                Salience = source.Salience,
                Category = source.Category,
                Text = partialText,
                TimestampTick = source.TimestampTick,
                Floor = source.Floor,
                IsBookmarked = source.IsBookmarked,
                IsPending = isPending,
                UsedLlm = source.UsedLlm,
                PerspectiveCharacterId = source.PerspectiveCharacterId
            };
        }
    }
}
