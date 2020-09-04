using RaceControl.Common.Enum;

namespace RaceControl.Common.Interfaces
{
    public interface IPlayableContent
    {
        string Title { get; }
        string Name { get; }
        ContentType ContentType { get; }
        string ContentUrl { get; }
        bool IsLive { get; }
        string SyncUID { get; }
        string DriverUID { get; }
    }
}