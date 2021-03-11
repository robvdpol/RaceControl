using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Action
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("targetType")]
        public string TargetType { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }
}