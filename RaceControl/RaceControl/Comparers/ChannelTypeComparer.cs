using RaceControl.Services.Interfaces.F1TV.Api;
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

            if (x == ChannelTypes.WIF)
            {
                return -1;
            }

            if (y == ChannelTypes.WIF)
            {
                return 1;
            }

            if (x == ChannelTypes.OTHER)
            {
                return -1;
            }

            if (y == ChannelTypes.OTHER)
            {
                return 1;
            }

            if (x == ChannelTypes.DRIVER)
            {
                return -1;
            }

            if (y == ChannelTypes.DRIVER)
            {
                return 1;
            }

            if (x == ChannelTypes.BACKUP)
            {
                return 1;
            }

            if (y == ChannelTypes.BACKUP)
            {
                return -1;
            }

            return 0;
        }
    }
}