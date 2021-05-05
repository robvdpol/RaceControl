using System.Collections.Generic;

namespace RaceControl.Common.Constants
{
    public static class LanguageCodes
    {
        public const string English = "eng";
        public const string German = "ger";
        public const string French = "fre";
        public const string Spanish = "spa";
        public const string Dutch = "dut";
        public const string Portuguese = "por";
        public const string Undetermined = "und";

        public static readonly IDictionary<string, string> AlternativeCodes = new Dictionary<string, string>
        {
            { "nld", Dutch },
            { "deu", German },
            { "fra", French },
            { "cfx", Undetermined },
            { "obc", Undetermined }
        };
    }
}