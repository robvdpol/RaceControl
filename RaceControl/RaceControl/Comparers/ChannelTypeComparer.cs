using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaceControl.Comparers
{
    public class ChannelTypeComparer : IComparer<string>
    {
        private const string Wif = "wif";
        private const string Driver = "driver";
        private const string Other = "other";

        public int Compare([AllowNull] string x, [AllowNull] string y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x == Wif)
            {
                return -1;
            }

            if (y == Wif)
            {
                return 1;
            }

            if (x == Other)
            {
                return -1;
            }

            if (y == Other)
            {
                return 1;
            }

            if (x == Driver)
            {
                return -1;
            }

            if (y == Driver)
            {
                return 1;
            }

            return 0;
        }
    }
}