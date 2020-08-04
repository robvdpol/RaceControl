using Prism.Events;

namespace RaceControl.Events
{
    public class SyncSessionEvent : PubSubEvent<SyncSessionEventPayload>
    {
    }

    public class SyncSessionEventPayload
    {
        public SyncSessionEventPayload(long time)
        {
            Time = time;
        }

        public long Time { get; }
    }
}