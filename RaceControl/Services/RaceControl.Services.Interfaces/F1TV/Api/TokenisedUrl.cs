using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class TokenisedUrl
    {
        [JsonProperty("tokenised_url")]
        public string Url { get; set; }
    }
}