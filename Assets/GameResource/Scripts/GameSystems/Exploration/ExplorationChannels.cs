using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Simulation;
using R3;

namespace Backend.GameSystems.Exploration
{
    public static class ExplorationChannels
    {
        private static readonly Subject<LogEntry> LogAddedSubject = new();
        private static readonly Subject<LogEntry> LogUpdatedSubject = new();
        private static readonly Subject<LogEntry> LogStreamingSubject = new();
        private static readonly Subject<ExplorationState> StateChangedSubject = new();
        private static readonly Subject<ExplorationEndReason> ExplorationEndedSubject = new();

        public static Observable<LogEntry> OnLogAdded => LogAddedSubject;
        public static Observable<LogEntry> OnLogUpdated => LogUpdatedSubject;
        public static Observable<LogEntry> OnLogStreaming => LogStreamingSubject;
        public static Observable<ExplorationState> OnStateChanged => StateChangedSubject;
        public static Observable<ExplorationEndReason> OnExplorationEnded => ExplorationEndedSubject;

        internal static void PublishLogAdded(LogEntry entry)
        {
            LogAddedSubject.OnNext(entry);
        }

        internal static void PublishLogUpdated(LogEntry entry)
        {
            LogUpdatedSubject.OnNext(entry);
        }

        internal static void PublishLogStreaming(LogEntry entry)
        {
            LogStreamingSubject.OnNext(entry);
        }

        internal static void PublishStateChanged(ExplorationState state)
        {
            StateChangedSubject.OnNext(state);
        }

        internal static void PublishExplorationEnded(ExplorationEndReason reason)
        {
            ExplorationEndedSubject.OnNext(reason);
        }
    }
}
