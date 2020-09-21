using RaceControl.Common.Enums;
using RaceControl.Services.Interfaces.F1TV.Api;
using System.Linq;

namespace RaceControl
{
    public class PlayableEpisode : PlayableContent
    {
        public PlayableEpisode(Episode episode)
        {
            Title = episode.Title;
            Name = episode.Title;
            DisplayName = episode.Title;
            ContentType = ContentType.Asset;
            ContentUrl = episode.Items?.FirstOrDefault();
            ThumbnailUrl = episode.ImageUrls?.FirstOrDefault(img => img.ImageType == "Thumbnail")?.Url;
            SyncUID = episode.UID;
        }
    }
}