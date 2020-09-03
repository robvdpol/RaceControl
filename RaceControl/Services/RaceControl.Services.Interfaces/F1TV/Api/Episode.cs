using Newtonsoft.Json;
using RaceControl.Common.Utils;
using System.Collections.Generic;
using System.Linq;

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

        [JsonProperty("image_urls")]
        public List<Image> ImageUrls { get; set; }

        [JsonIgnore]
        public string ThumbnailUrl => ImageUrls?.FirstOrDefault(img => img.ImageType == "Thumbnail")?.Url;

        public static string UIDField => JsonUtils.GetJsonPropertyName<Episode>(e => e.UID);
        public static string TitleField => JsonUtils.GetJsonPropertyName<Episode>(e => e.Title);
        public static string ItemsField => JsonUtils.GetJsonPropertyName<Episode>(e => e.Items);
        public static string ImageUrlsField => JsonUtils.GetJsonPropertyName<Session>(s => s.ImageUrls);

        public override string ToString()
        {
            return Title;
        }
    }
}