using RaceControl.Common.Enums;
using RaceControl.Services.Interfaces.F1TV.Api;

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
            ContentUrl = channel.PlaybackUrl;
            IsLive = session.IsLive;
            SyncUID = session.UID;
        }

        private static string GetDisplayName(Channel channel)
        {
            switch (channel.Name)
            {
                case "WIF":
                    return "World Feed";

                case "PIT LANE":
                    return "Pit Lane";

                case "TRACKER":
                    return "Driver Tracker";

                case "DATA":
                    return "Live Timing";

                default:
                    return channel.Name;
            }
        }

        private static ContentType GetContentType(Channel channel)
        {
            return channel.ChannelType == ChannelTypes.Backup ? ContentType.Backup : ContentType.Channel;
        }
    }
}