using RaceControl.Common.Enums;
using RaceControl.Services.Interfaces.F1TV.Api;

namespace RaceControl
{
    public class PlayableEpisode : PlayableContent
    {
        public PlayableEpisode(Episode episode)
        {
            Title = $"{episode.SessionName} - {episode.Name}";
            Name = episode.Name;
            DisplayName = episode.Name;
            ContentType = ContentType.Asset;
            ContentUrl = episode.PlaybackUrl;
            ThumbnailUrl = episode.ThumbnailUrl;
            SyncUID = episode.UID;
        }
    }
}