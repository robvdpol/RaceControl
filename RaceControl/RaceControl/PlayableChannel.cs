namespace RaceControl;

public class PlayableChannel : PlayableContent
{
    public PlayableChannel(Season season, Session session, Channel channel)
    {
        var displayName = GetDisplayName(season, channel);

        Title = $"{session.LongName} - {displayName}";
        Name = channel.Name;
        DisplayName = displayName;
        ContentType = ContentType.Channel;
        ContentUrl = channel.PlaybackUrl;
        IsLive = session.IsLive;
        SyncUID = session.UID;
        SeriesUID = session.SeriesUID;
        RequiredSubscriptionLevel = channel.RequiredSubcriptionLevel ?? "Pro";
    }

    private static string GetDisplayName(Season season, Channel channel)
    {
        switch (channel.Name)
        {
            case ChannelNames.Wif:
                return "International";

            case ChannelNames.PitLane:
                return season.Year >= 2022 ? "F1 Live" : "Pit Lane";

            case ChannelNames.Tracker:
                return "Driver Tracker";

            case ChannelNames.Data:
                return "Live Timing";

            default:
                return channel.Name;
        }
    }
}