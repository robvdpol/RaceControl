using Newtonsoft.Json;
using RaceControl.Common;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
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

        public static string UIDField => JsonUtils.GetJsonPropertyName<Season>((s) => s.UID);
        public static string NameField => JsonUtils.GetJsonPropertyName<Season>((s) => s.Name);
        public static string HasContentField => JsonUtils.GetJsonPropertyName<Season>((s) => s.HasContent);
        public static string YearField => JsonUtils.GetJsonPropertyName<Season>((s) => s.Year);
        public static string EventOccurrenceUrlsField => JsonUtils.GetJsonPropertyName<Season>((s) => s.EventOccurrenceUrls);
    }
}