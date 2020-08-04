using RaceControl.Services.Interfaces.F1TV.Api;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaceControl.Comparers
{
    public class ChannelComparer : IComparer<string>
    {
        public int Compare([AllowNull] string x, [AllowNull] string y)
        {
            if (x == Channel.WIF)
            {
                return -1;
            }

            if (y == Channel.WIF)
            {
                return 1;
            }

            if (x == Channel.PitLane)
            {
                return -1;
            }

            if (y == Channel.PitLane)
            {
                return 1;
            }

            if (x == Channel.Driver)
            {
                return -1;
            }

            if (y == Channel.Driver)
            {
                return 1;
            }

            if (x == Channel.Data)
            {
                return -1;
            }

            if (y == Channel.Data)
            {
                return 1;
            }

            return 0;
        }
    }
}