namespace RaceControl.Events;

public class SyncStreamsEvent : PubSubEvent<SyncStreamsEventPayload>
{
}

public class SyncStreamsEventPayload
{
    public SyncStreamsEventPayload(string syncUID, long time)
    {
        SyncUID = syncUID;
        Time = time;
    }

    public string SyncUID { get; }
    public long Time { get; }
}