using RaceControl.Common.Enums;
using RaceControl.Services.Interfaces.F1TV.Entities;

namespace RaceControl
{
    public class PlayableEpisode : PlayableContent
    {
        public PlayableEpisode(Episode episode)
        {
            Title = episode.LongName;
            Name = episode.ShortName;
            DisplayName = episode.LongName;
            ContentType = ContentType.Asset;
            ContentUrl = episode.PlaybackUrl;
            ThumbnailUrl = episode.ThumbnailUrl;
            SyncUID = episode.UID;
            SeriesUID = episode.SeriesUID;
        }
    }
}