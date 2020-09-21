using RaceControl.Common.Enums;
using RaceControl.Common.Utils;
using RaceControl.Services.Interfaces.F1TV.Api;
using System.Linq;

namespace RaceControl
{
    public class PlayableChannel : PlayableContent
    {
        public PlayableChannel(Session session, Channel channel)
        {
            var displayName = GetDisplayName(channel);

            Title = $"{session.SessionName} - {displayName}";
            Name = channel.Name;
            DisplayName = displayName;
            ContentType = GetContentType(channel);
            ContentUrl = channel.Self;
            IsLive = session.IsLive;
            SyncUID = session.UID;
            DriverUID = channel.DriverOccurrenceUrls?.FirstOrDefault().GetUID();
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

        private static ContentType GetContentType(Channel channel)
        {
            return channel.ChannelType == ChannelTypes.BACKUP ? ContentType.Backup : ContentType.Channel;
        }
    }
}