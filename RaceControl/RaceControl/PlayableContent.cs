namespace RaceControl;

public abstract class PlayableContent : IPlayableContent
{
    public string Title { get; protected init; }
    public string Name { get; protected init; }
    public string DisplayName { get; protected init; }
    public ContentType ContentType { get; protected init; }
    public string ContentUrl { get; protected init; }
    public string ThumbnailUrl { get; protected init; }
    public bool IsLive { get; protected init; }
    public string SyncUID { get; protected init; }
    public string SeriesUID { get; protected init; }

    public override string ToString()
    {
        return DisplayName;
    }
}