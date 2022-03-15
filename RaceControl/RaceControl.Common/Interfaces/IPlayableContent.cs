using RaceControl.Common.Enums;

namespace RaceControl.Common.Interfaces;

public interface IPlayableContent
{
    string Title { get; }
    string Name { get; }
    string DisplayName { get; }
    ContentType ContentType { get; }
    string ContentUrl { get; }
    string ThumbnailUrl { get; }
    bool IsLive { get; }
    string SyncUID { get; }
    string SeriesUID { get; }
}