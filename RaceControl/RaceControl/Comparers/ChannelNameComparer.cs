using System.Diagnostics.CodeAnalysis;

namespace RaceControl.Comparers;

public class ChannelNameComparer : IComparer<string>
{
    public int Compare([AllowNull] string x, [AllowNull] string y)
    {
        if (x == y)
        {
            return 0;
        }

        // International
        if (x == ChannelNames.Wif)
        {
            return -1;
        }

        if (y == ChannelNames.Wif)
        {
            return 1;
        }

        // F1 Live
        if (x == ChannelNames.PitLane)
        {
            return -1;
        }

        if (y == ChannelNames.PitLane)
        {
            return 1;
        }

        // Live Timing
        if (x == ChannelNames.Data)
        {
            return -1;
        }

        if (y == ChannelNames.Data)
        {
            return 1;
        }

        // Driver Tracker
        if (x == ChannelNames.Tracker)
        {
            return -1;
        }

        if (y == ChannelNames.Tracker)
        {
            return 1;
        }

        // Driver Names
        return string.CompareOrdinal(x, y);
    }
}