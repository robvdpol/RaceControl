using Newtonsoft.Json;
using RaceControl.Common;
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

        public static string UIDField => JsonUtils.GetJsonPropertyName<Event>((s) => s.UID);
        public static string NameField => JsonUtils.GetJsonPropertyName<Event>((s) => s.Name);
        public static string OfficialNameField => JsonUtils.GetJsonPropertyName<Event>((s) => s.OfficialName);
        public static string SessionOccurrenceUrlsField => JsonUtils.GetJsonPropertyName<Event>((s) => s.SessionOccurrenceUrls);
        public static string StartDateField => JsonUtils.GetJsonPropertyName<Event>((s) => s.StartDate);
        public static string EndDateField => JsonUtils.GetJsonPropertyName<Event>((s) => s.EndDate);

        public override string ToString()
        {
            return Name;
        }
    }
}