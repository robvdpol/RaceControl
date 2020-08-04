using Newtonsoft.Json;
using RaceControl.Common;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Episode
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("data_source_id")]
        public string DataSourceID { get; set; }

        [JsonProperty("items")]
        public List<string> Items { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<Episode>((e) => e.UID);
        public static string TitleField => JsonUtils.GetJsonPropertyName<Episode>((e) => e.Title);
        public static string SubtitleField => JsonUtils.GetJsonPropertyName<Episode>((e) => e.Subtitle);
        public static string DataSourceIDField => JsonUtils.GetJsonPropertyName<Episode>((e) => e.DataSourceID);
        public static string ItemsField => JsonUtils.GetJsonPropertyName<Episode>((e) => e.Items);

        public override string ToString()
        {
            return Title;
        }
    }
}