using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RaceControl.Services.F1TV.Api
{
    public class Session
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("session_name")]
        public string SessionName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("content_urls")]
        public List<string> ContentUrls { get; set; }

        [JsonProperty("start_time")]
        public DateTime? StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime? EndTime { get; set; }
    }
}