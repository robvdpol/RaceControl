using Newtonsoft.Json;
using RaceControl.Common.Utils;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Episode
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("items")]
        public List<string> Items { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<Episode>(e => e.UID);
        public static string TitleField => JsonUtils.GetJsonPropertyName<Episode>(e => e.Title);
        public static string ItemsField => JsonUtils.GetJsonPropertyName<Episode>(e => e.Items);

        public override string ToString()
        {
            return Title;
        }
    }
}