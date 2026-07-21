using Backend.GameSystems.DynamicEvent.Data;
using R3;

namespace Backend.GameSystems.DynamicEvent
{
    public static class DynamicEventChannels
    {
        private static readonly Subject<DynamicEventInstance> StartedSubject = new();
        private static readonly Subject<DynamicEventInstance> ResolvedSubject = new();

        public static Observable<DynamicEventInstance> OnEventStarted => StartedSubject;
        public static Observable<DynamicEventInstance> OnEventResolved => ResolvedSubject;

        internal static void PublishEventStarted(DynamicEventInstance instance)
        {
            StartedSubject.OnNext(instance);
        }

        internal static void PublishEventResolved(DynamicEventInstance instance)
        {
            ResolvedSubject.OnNext(instance);
        }
    }
}
