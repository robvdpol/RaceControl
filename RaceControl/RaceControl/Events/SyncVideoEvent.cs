using Prism.Events;

namespace RaceControl.Events
{
    public class SyncVideoEvent : PubSubEvent<SyncVideoEventPayload>
    {
    }

    public class SyncVideoEventPayload
    {
        public SyncVideoEventPayload(long time)
        {
            Time = time;
        }

        public long Time { get; }
    }
}