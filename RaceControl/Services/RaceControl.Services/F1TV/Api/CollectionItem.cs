using Newtonsoft.Json;

namespace RaceControl.Services.F1TV.Api
{
    public class CollectionItem
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("content_url")]
        public string ContentURL { get; set; }

        [JsonProperty("display_type")]
        public string DisplayType { get; set; }

        [JsonProperty("set_url")]
        public string SetURL { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }
    }
}