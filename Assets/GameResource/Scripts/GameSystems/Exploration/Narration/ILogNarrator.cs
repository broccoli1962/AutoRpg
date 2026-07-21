using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Narration
{
    public interface ILogNarrator
    {
        LogEntry Narrate(ExplorationEvent explorationEvent, PartyState party);
        LogEntry NarrateOfflineSummary(OfflineSummaryContext context);
    }

    public sealed class OfflineSummaryContext
    {
        public long SimulatedTicks;
        public int EventCount;
        public int GoldGained;
        public int StartFloor;
        public int EndFloor;
        public string ZoneDisplayName;
    }
}
