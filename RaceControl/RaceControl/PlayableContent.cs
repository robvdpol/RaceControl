using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;

namespace RaceControl
{
    public abstract class PlayableContent : IPlayableContent
    {
        public string Title { get; protected set; }
        public string Name { get; protected set; }
        public string DisplayName { get; protected set; }
        public ContentType ContentType { get; protected set; }
        public string ContentUrl { get; protected set; }
        public string ThumbnailUrl { get; protected set; }
        public bool IsLive { get; protected set; }
        public string SyncUID { get; protected set; }
        public string DriverUID { get; protected set; }
        public string SeriesUID { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}