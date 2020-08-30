using Newtonsoft.Json;
using RaceControl.Common.Utils;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Image
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image_type")]
        public string ImageType { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<Image>(i => i.UID);
        public static string TitleField => JsonUtils.GetJsonPropertyName<Image>(i => i.Title);
        public static string ImageTypeField => JsonUtils.GetJsonPropertyName<Image>(i => i.ImageType);
        public static string UrlField => JsonUtils.GetJsonPropertyName<Image>(i => i.Url);
    }
}