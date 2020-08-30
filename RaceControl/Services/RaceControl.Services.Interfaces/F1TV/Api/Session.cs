using Newtonsoft.Json;
using RaceControl.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Session
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("session_name")]
        public string SessionName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime EndTime { get; set; }

        [JsonProperty("eventoccurrence_url")]
        public string EventOccurrenceUrl { get; set; }

        [JsonProperty("series_url")]
        public string SeriesUrl { get; set; }

        [JsonProperty("content_urls")]
        public List<Episode> ContentUrls { get; set; }

        [JsonProperty("channel_urls")]
        public List<Channel> ChannelUrls { get; set; }

        [JsonProperty("image_urls")]
        public List<Image> ImageUrls { get; set; }

        [JsonIgnore]
        public bool IsUpcoming => Status == "upcoming";

        [JsonIgnore]
        public bool IsLive => Status == "live";

        [JsonIgnore]
        public bool IsExpired => Status == "expired";

        [JsonIgnore]
        public bool IsReplay => Status == "replay";

        [JsonIgnore]
        public string ThumbnailUrl => ImageUrls?.FirstOrDefault(img => img.ImageType == "Thumbnail")?.Url;

        public static string UIDField => JsonUtils.GetJsonPropertyName<Session>(s => s.UID);
        public static string NameField => JsonUtils.GetJsonPropertyName<Session>(s => s.Name);
        public static string SessionNameField => JsonUtils.GetJsonPropertyName<Session>(s => s.SessionName);
        public static string StatusField => JsonUtils.GetJsonPropertyName<Session>(s => s.Status);
        public static string StartTimeField => JsonUtils.GetJsonPropertyName<Session>(s => s.StartTime);
        public static string EndTimeField => JsonUtils.GetJsonPropertyName<Session>(s => s.EndTime);
        public static string EventOccurrenceUrlField => JsonUtils.GetJsonPropertyName<Session>(s => s.EventOccurrenceUrl);
        public static string SeriesUrlField => JsonUtils.GetJsonPropertyName<Session>(s => s.SeriesUrl);
        public static string ContentUrlsField => JsonUtils.GetJsonPropertyName<Session>(s => s.ContentUrls);
        public static string ChannelUrlsField => JsonUtils.GetJsonPropertyName<Session>(s => s.ChannelUrls);
        public static string ImageUrlsField => JsonUtils.GetJsonPropertyName<Session>(s => s.ImageUrls);

        public override string ToString()
        {
            if (IsUpcoming)
            {
                return $"{Name} (upcoming)";
            }

            if (IsLive)
            {
                return $"{Name} (live)";
            }

            if (IsExpired)
            {
                return $"{Name} (expired)";
            }

            return Name;
        }
    }
}