using Newtonsoft.Json;
using RaceControl.Common.Utils;
using System;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Event
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }

        [JsonProperty("race_season_url")]
        public string RaceSeasonUrl { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<Event>(e => e.UID);
        public static string NameField => JsonUtils.GetJsonPropertyName<Event>(e => e.Name);
        public static string StartDateField => JsonUtils.GetJsonPropertyName<Event>(e => e.StartDate);
        public static string EndDateField => JsonUtils.GetJsonPropertyName<Event>(e => e.EndDate);
        public static string RaceSeasonUrlField => JsonUtils.GetJsonPropertyName<Event>(e => e.RaceSeasonUrl);

        public override string ToString()
        {
            return Name;
        }
    }
}