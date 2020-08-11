using Newtonsoft.Json;
using RaceControl.Common.Utils;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class VodType
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content_urls")]
        public List<string> ContentUrls { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<VodType>((v) => v.UID);
        public static string NameField => JsonUtils.GetJsonPropertyName<VodType>((v) => v.Name);
        public static string ContentUrlsField => JsonUtils.GetJsonPropertyName<VodType>((v) => v.ContentUrls);

        public override string ToString()
        {
            return Name;
        }
    }
}