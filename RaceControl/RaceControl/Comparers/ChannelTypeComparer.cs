using RaceControl.Common.Constants;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaceControl.Comparers
{
    public class ChannelTypeComparer : IComparer<string>
    {
        public int Compare([AllowNull] string x, [AllowNull] string y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x == ChannelTypes.Wif)
            {
                return -1;
            }

            if (y == ChannelTypes.Wif)
            {
                return 1;
            }

            if (x == ChannelTypes.Additional)
            {
                return -1;
            }

            if (y == ChannelTypes.Additional)
            {
                return 1;
            }

            if (x == ChannelTypes.Onboard)
            {
                return -1;
            }

            if (y == ChannelTypes.Onboard)
            {
                return 1;
            }

            if (x == ChannelTypes.Backup)
            {
                return 1;
            }

            if (y == ChannelTypes.Backup)
            {
                return -1;
            }

            return 0;
        }
    }
}