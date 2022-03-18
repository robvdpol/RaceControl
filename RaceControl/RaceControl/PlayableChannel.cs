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
            case ChannelNames.Wif:
                return "International";

            case ChannelNames.PitLane:
                return "F1 Live";

            case ChannelNames.Tracker:
                return "Driver Tracker";

            case ChannelNames.Data:
                return "Live Timing";

            default:
                return channel.Name;
        }
    }
}