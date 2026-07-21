using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;

namespace Backend.GameSystems.LLM
{
    public sealed class LlmNarrationJob
    {
        public ExplorationEvent Event;
        public PartyState Party;
        public LogEntry PendingEntry;
        public LogEntry FallbackEntry;
    }
}
