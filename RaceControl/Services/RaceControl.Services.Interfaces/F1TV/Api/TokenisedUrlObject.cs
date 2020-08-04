using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class TokenisedUrlObject
    {
        [JsonProperty("tata")]
        public TokenisedUrl TokenisedUrl { get; set; }
    }
}