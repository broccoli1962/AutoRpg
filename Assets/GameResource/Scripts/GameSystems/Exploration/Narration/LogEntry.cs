using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Narration
{
    public sealed class LogEntry
    {
        public string EventId;
        public EventType EventType;
        public SalienceGrade Salience;
        public LogCategory Category;
        public string Text;
        public long TimestampTick;
        public int Floor;
        public bool IsBookmarked;
    }
}
