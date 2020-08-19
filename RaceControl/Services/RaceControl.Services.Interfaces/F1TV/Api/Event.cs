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
        public DateTime? StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("race_season_url")]
        public string RaceSeasonUrl { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<Event>(s => s.UID);
        public static string NameField => JsonUtils.GetJsonPropertyName<Event>(s => s.Name);
        public static string StartDateField => JsonUtils.GetJsonPropertyName<Event>(s => s.StartDate);
        public static string EndDateField => JsonUtils.GetJsonPropertyName<Event>(s => s.EndDate);
        public static string RaceSeasonUrlField => JsonUtils.GetJsonPropertyName<Event>(s => s.RaceSeasonUrl);

        public override string ToString()
        {
            return Name;
        }
    }
}