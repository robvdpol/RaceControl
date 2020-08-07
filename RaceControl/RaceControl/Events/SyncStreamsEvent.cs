using Prism.Events;
using System;

namespace RaceControl.Events
{
    public class SyncStreamsEvent : PubSubEvent<SyncStreamsEventPayload>
    {
    }

    public class SyncStreamsEventPayload
    {
        public SyncStreamsEventPayload(Guid requesterIdentifier, string syncUID, long time)
        {
            RequesterIdentifier = requesterIdentifier;
            SyncUID = syncUID;
            Time = time;
        }

        public Guid RequesterIdentifier { get; }
        public string SyncUID { get; }
        public long Time { get; }
    }
}