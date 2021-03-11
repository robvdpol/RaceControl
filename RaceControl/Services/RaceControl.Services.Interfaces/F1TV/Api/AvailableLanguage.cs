using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class AvailableLanguage
    {
        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; }

        [JsonProperty("languageName")]
        public string LanguageName { get; set; }
    }
}