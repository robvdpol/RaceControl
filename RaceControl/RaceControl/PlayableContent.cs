using RaceControl.Common.Enum;
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
        public ContentType ContentType { get; private set; }
        public string ContentUrl { get; private set; }
        public bool IsLive { get; private set; }
        public string SyncUID { get; private set; }
        public string DriverUID { get; private set; }

        public static IPlayableContent Create(Episode episode)
        {
            return new PlayableContent
            {
                Title = episode.Title,
                Name = episode.Title,
                ContentType = ContentType.Asset,
                ContentUrl = episode.Items?.FirstOrDefault(),
                IsLive = false,
                SyncUID = episode.UID
            };
        }

        public static IPlayableContent Create(Session session, Channel channel)
        {
            return new PlayableContent
            {
                Title = $"{session.SessionName} - {channel}",
                Name = channel.Name,
                ContentType = channel.ChannelType == ChannelTypes.BACKUP ? ContentType.Backup : ContentType.Channel,
                ContentUrl = channel.Self,
                IsLive = session.IsLive,
                SyncUID = session.UID,
                DriverUID = channel.DriverOccurrenceUrls.FirstOrDefault().GetUID()
            };
        }
    }
}