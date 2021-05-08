using System.Collections.Generic;

namespace RaceControl.Common.Constants
{
    public static class LanguageCodes
    {
        public const string English = "eng";
        public const string German = "deu";
        public const string French = "fra";
        public const string Spanish = "spa";
        public const string Portuguese = "por";
        public const string Dutch = "nld";
        public const string SoundFx = "cfx";
        public const string Onboard = "obc";
        public const string Undetermined = "und";

        private static readonly IDictionary<string, string> FlyleafCodes = new Dictionary<string, string>
        {
            { German, "ger" },
            { French, "fre" },
            { Dutch, "dut" },
            { Onboard, "und" },
            { SoundFx, "und" }
        };

        public static string GetFlyleafCode(string languageCode)
        {
            return FlyleafCodes.TryGetValue(languageCode, out var flyleafCode) ? flyleafCode : languageCode;
        }

        public static string GetStandardCode(string flyleafCode)
        {
            if (string.IsNullOrWhiteSpace(flyleafCode))
            {
                return English;
            }

            foreach (var (key, value) in FlyleafCodes)
            {
                if (value == flyleafCode)
                {
                    return key;
                }
            }

            return flyleafCode;
        }
    }
}