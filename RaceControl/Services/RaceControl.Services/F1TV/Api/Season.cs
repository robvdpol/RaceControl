using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceControl.Services.F1TV.Api
{
    public class Season
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("has_content")]
        public bool HasContent { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("eventoccurrence_urls")]
        public List<string> EventOccurrenceUrls { get; set; }
    }
}