using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Event
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("official_name")]
        public string OfficialName { get; set; }

        [JsonProperty("sessionoccurrence_urls")]
        public List<string> SessionOccurrenceUrls { get; set; }

        [JsonProperty("start_date")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }
    }
}