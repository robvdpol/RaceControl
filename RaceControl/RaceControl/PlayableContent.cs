using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Utils;
using RaceControl.Services.Interfaces.F1TV.Api;
using System.Linq;

namespace RaceControl
{
    public class PlayableContent : IPlayableContent
    {
        private PlayableContent()
        {
        }

        public string Title { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public ContentType ContentType { get; private set; }
        public string ContentUrl { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public bool IsLive { get; private set; }
        public string SyncUID { get; private set; }
        public string DriverUID { get; private set; }

        public static IPlayableContent Create(Session session, Channel channel)
        {
            var displayName = GetDisplayName(channel);

            return new PlayableContent
            {
                Title = $"{session.SessionName} - {displayName}",
                Name = channel.Name,
                DisplayName = displayName,
                ContentType = channel.ChannelType == ChannelTypes.BACKUP ? ContentType.Backup : ContentType.Channel,
                ContentUrl = channel.Self,
                ThumbnailUrl = null,
                IsLive = session.IsLive,
                SyncUID = session.UID,
                DriverUID = channel.DriverOccurrenceUrls?.FirstOrDefault().GetUID()
            };
        }

        public static IPlayableContent Create(Episode episode)
        {
            return new PlayableContent
            {
                Title = episode.Title,
                Name = episode.Title,
                DisplayName = episode.Title,
                ContentType = ContentType.Asset,
                ContentUrl = episode.Items?.FirstOrDefault(),
                ThumbnailUrl = episode.ImageUrls?.FirstOrDefault(img => img.ImageType == "Thumbnail")?.Url,
                IsLive = false,
                SyncUID = episode.UID,
                DriverUID = null
            };
        }

        private static string GetDisplayName(Channel channel)
        {
            switch (channel.Name)
            {
                case "WIF":
                    return "World Feed";

                case "pit lane":
                    return "Pit Lane";

                case "driver":
                    return "Driver Tracker";

                case "data":
                    return "Live Timing";

                default:
                    return channel.Name;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}