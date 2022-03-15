namespace RaceControl.Services.Interfaces.F1TV.Api;

public class AvailableLanguage
{
    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; }

    [JsonPropertyName("languageName")]
    public string LanguageName { get; set; }
}