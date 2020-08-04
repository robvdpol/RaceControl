using Prism.Events;

namespace RaceControl.Events
{
    public class SyncSessionEvent : PubSubEvent<SyncSessionEventPayload>
    {
    }

    public class SyncSessionEventPayload
    {
        public SyncSessionEventPayload(string sessionUID, long time)
        {
            SessionUID = sessionUID;
            Time = time;
        }

        public string SessionUID { get; }
        public long Time { get; }
    }
}