namespace RaceControl;

public class PlayableChannel : PlayableContent
{
    public PlayableChannel(Session session, Channel channel)
    {
        var displayName = GetDisplayName(channel);

        Title = $"{session.LongName} - {displayName}";
        Name = channel.Name;
        DisplayName = displayName;
        ContentType = ContentType.Channel;
        ContentUrl = channel.PlaybackUrl;
        IsLive = session.IsLive;
        SyncUID = session.UID;
        SeriesUID = session.SeriesUID;
    }

    private static string GetDisplayName(Channel channel)
    {
        switch (channel.Name)
        {
            case ChannelNames.International:
                return "International";

            case ChannelNames.Wif:
                return "World Feed";

            case ChannelNames.F1Live:
                return "F1 Live";

            case ChannelNames.PitLane:
                return "Pit Lane";

            case ChannelNames.Tracker:
                return "Driver Tracker";

            case ChannelNames.Data:
                return "Live Timing";

            default:
                return channel.Name;
        }
    }
}