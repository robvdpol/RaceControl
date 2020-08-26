using Newtonsoft.Json;
using RaceControl.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Series
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content_urls")]
        public List<string> ContentUrls { get; set; }

        [JsonProperty("sessionoccurrence_urls")]
        public List<string> SessionOccurrenceUrls { get; set; }

        [JsonIgnore]
        public bool HasContent => ContentUrls.Any() || SessionOccurrenceUrls.Any();

        public static string UIDField => JsonUtils.GetJsonPropertyName<Series>(s => s.UID);
        public static string SelfField => JsonUtils.GetJsonPropertyName<Series>(s => s.Self);
        public static string NameField => JsonUtils.GetJsonPropertyName<Series>(s => s.Name);
        public static string ContentUrlsField => JsonUtils.GetJsonPropertyName<Series>(s => s.ContentUrls);
        public static string SessionOccurrenceUrlsField => JsonUtils.GetJsonPropertyName<Series>(s => s.SessionOccurrenceUrls);

        public override string ToString()
        {
            return Name;
        }
    }
}