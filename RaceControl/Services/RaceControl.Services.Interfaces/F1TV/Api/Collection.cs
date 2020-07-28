using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Collection
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("unique_items")]
        public bool UniqueItems { get; set; }

        [JsonProperty("items")]
        public List<CollectionItem> Items { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }
    }
}