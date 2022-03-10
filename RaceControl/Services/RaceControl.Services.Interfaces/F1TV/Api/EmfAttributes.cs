using Newtonsoft.Json;
using System;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class EmfAttributes
    {
        [JsonProperty("VideoType")]
        public string VideoType { get; set; }

        [JsonProperty("MeetingKey")]
        public string MeetingKey { get; set; }

        [JsonProperty("MeetingSessionKey")]
        public string MeetingSessionKey { get; set; }

        [JsonProperty("Meeting_Name")]
        public string MeetingName { get; set; }

        [JsonProperty("Meeting_Number")]
        public string MeetingNumber { get; set; }

        [JsonProperty("Circuit_Short_Name")]
        public string CircuitShortName { get; set; }

        [JsonProperty("Meeting_Code")]
        public string MeetingCode { get; set; }

        [JsonProperty("MeetingCountryKey")]
        public string MeetingCountryKey { get; set; }

        [JsonProperty("CircuitKey")]
        public string CircuitKey { get; set; }

        [JsonProperty("Meeting_Location")]
        public string MeetingLocation { get; set; }

        [JsonProperty("Series")]
        public string Series { get; set; }

        [JsonProperty("OBC")]
        public bool OBC { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("TimetableKey")]
        public string TimetableKey { get; set; }

        [JsonProperty("SessionKey")]
        public string SessionKey { get; set; }

        [JsonProperty("SessionPeriod")]
        public string SessionPeriod { get; set; }

        [JsonProperty("Circuit_Official_Name")]
        public string CircuitOfficialName { get; set; }

        [JsonProperty("ActivityDescription")]
        public string ActivityDescription { get; set; }

        [JsonProperty("SeriesMeetingSessionIdentifier")]
        public string SeriesMeetingSessionIdentifier { get; set; }

        [JsonProperty("sessionEndTime")]
        public string SessionEndTime { get; set; }

        [JsonProperty("Meeting_Start_Date")]
        public DateTime? MeetingStartDate { get; set; }

        [JsonProperty("Meeting_End_Date")]
        public DateTime? MeetingEndDate { get; set; }

        [JsonProperty("Track_Length")]
        public string TrackLength { get; set; }

        [JsonProperty("Scheduled_Lap_Count")]
        public string ScheduledLapCount { get; set; }

        [JsonProperty("Scheduled_Distance")]
        public string ScheduledDistance { get; set; }

        [JsonProperty("Circuit_Location")]
        public string CircuitLocation { get; set; }

        [JsonProperty("Meeting_Sponsor")]
        public string MeetingSponsor { get; set; }

        [JsonProperty("IsTestEvent")]
        public string IsTestEvent { get; set; }

        [JsonProperty("Season_Meeting_Ordinal")]
        public int SeasonMeetingOrdinal { get; set; }

        [JsonProperty("Championship_Meeting_Ordinal")]
        public string ChampionshipMeetingOrdinal { get; set; }

        [JsonProperty("session_index")]
        public int SessionIndex { get; set; }

        [JsonProperty("Meeting_Official_Name")]
        public string MeetingOfficialName { get; set; }

        [JsonProperty("Meeting_Display_Date")]
        public string MeetingDisplayDate { get; set; }

        [JsonProperty("PageID")]
        public int PageID { get; set; }

        [JsonProperty("Meeting_Country_Name")]
        public string MeetingCountryName { get; set; }

        [JsonProperty("sessionEndDate")]
        public long SessionEndDate { get; set; }

        [JsonProperty("sessionStartDate")]
        public long SessionStartDate { get; set; }

        [JsonProperty("Global_Title")]
        public string GlobalTitle { get; set; }

        [JsonProperty("Global_Meeting_Country_Name")]
        public string GlobalMeetingCountryName { get; set; }

        [JsonProperty("Global_Meeting_Name")]
        public string GlobalMeetingName { get; set; }
    }
}