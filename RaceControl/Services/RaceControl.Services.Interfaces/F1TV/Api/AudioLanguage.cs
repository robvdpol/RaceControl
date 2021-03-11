using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class AudioLanguage
    {
        [JsonProperty("audioLanguageName")]
        public string AudioLanguageName { get; set; }

        [JsonProperty("audioId")]
        public string AudioId { get; set; }

        [JsonProperty("isPreferred")]
        public bool IsPreferred { get; set; }
    }
}