using Newtonsoft.Json;

namespace RaceControl.Services.F1TV.Api
{
    public class Channel
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}