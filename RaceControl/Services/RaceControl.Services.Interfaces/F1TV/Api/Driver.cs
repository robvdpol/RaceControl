using Newtonsoft.Json;
using RaceControl.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Driver
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_urls")]
        public List<Image> ImageUrls { get; set; }

        [JsonIgnore]
        public string HeadshotUrl => ImageUrls?.FirstOrDefault(img => img.ImageType == "Headshot")?.Url;

        [JsonIgnore]
        public string HelmetUrl => ImageUrls?.FirstOrDefault(img => img.ImageType == "Helmet")?.Url;

        [JsonIgnore]
        public string CarUrl => ImageUrls?.FirstOrDefault(img => img.ImageType == "Car")?.Url;

        public static string UIDField => JsonUtils.GetJsonPropertyName<Driver>(d => d.UID);
        public static string NameField => JsonUtils.GetJsonPropertyName<Driver>(d => d.Name);
        public static string ImageUrlsField => JsonUtils.GetJsonPropertyName<Driver>(d => d.ImageUrls);

        public override string ToString()
        {
            return Name;
        }
    }
}