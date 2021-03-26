using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RaceControl.Common.Constants
{
    public static class SeriesNames
    {
        public const string Formula1 = "Formula 1";
        public const string Formula2 = "Formula 2";
        public const string Formula3 = "Formula 3";
        public const string PorscheSupercup = "Porsche Supercup";

        private static readonly Dictionary<string, string[]> shortNames = new()
        {
            { SeriesIds.Formula1, new[] { "F1" } },
            { SeriesIds.Formula2, new[] { "F2" } },
            { SeriesIds.Formula3, new[] { "F3" } },
            { SeriesIds.PorscheSupercup, new[] { "Supercup", "PSC" } },
        };
        public static ReadOnlyDictionary<string, string[]> SeriesShortNames { get; } = new(shortNames);
    }    
}