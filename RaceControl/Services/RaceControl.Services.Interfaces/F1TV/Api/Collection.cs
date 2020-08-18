using Newtonsoft.Json;
using RaceControl.Common.Utils;
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

        public static string UIDField => JsonUtils.GetJsonPropertyName<Collection>(c => c.UID);
        public static string TitleField => JsonUtils.GetJsonPropertyName<Collection>(c => c.Title);
        public static string UniqueItemsField => JsonUtils.GetJsonPropertyName<Collection>(c => c.UniqueItems);
        public static string ItemsField => JsonUtils.GetJsonPropertyName<Collection>(c => c.Items);
        public static string SummaryField => JsonUtils.GetJsonPropertyName<Collection>(c => c.Summary);
    }
}