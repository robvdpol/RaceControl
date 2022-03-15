namespace RaceControl.Common.Constants;

public static class LanguageCodes
{
    public const string English = "eng";
    public const string German = "deu";
    public const string French = "fra";
    public const string Spanish = "spa";
    public const string Portuguese = "por";
    public const string Dutch = "nld";
    public const string Onboard = "teamradio";
    public const string Undetermined = "und";

    private static readonly IDictionary<string, string> FlyleafCodes = new Dictionary<string, string>
        {
            { German, "ger" },
            { French, "fre" },
            { Dutch, "dut" }
        };

    private static readonly IDictionary<string, string> TwoLetterCodes = new Dictionary<string, string>
        {
            { English, "en" },
            { German, "de" },
            { French, "fr" },
            { Spanish, "es" },
            { Portuguese, "pt" },
            { Dutch, "nl" }
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

    public static string GetTwoLetterCode(string languageCode)
    {
        return TwoLetterCodes.TryGetValue(languageCode, out var twoLetterCode) ? twoLetterCode : null;
    }
}