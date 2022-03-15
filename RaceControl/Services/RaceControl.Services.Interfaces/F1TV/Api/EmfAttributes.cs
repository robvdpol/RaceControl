namespace RaceControl.Services.Interfaces.F1TV.Api;

public class EmfAttributes
{
    [JsonPropertyName("VideoType")]
    public string VideoType { get; set; }

    [JsonPropertyName("MeetingKey")]
    public string MeetingKey { get; set; }

    [JsonPropertyName("MeetingSessionKey")]
    public string MeetingSessionKey { get; set; }

    [JsonPropertyName("Meeting_Name")]
    public string MeetingName { get; set; }

    [JsonPropertyName("Meeting_Number")]
    public string MeetingNumber { get; set; }

    [JsonPropertyName("Circuit_Short_Name")]
    public string CircuitShortName { get; set; }

    [JsonPropertyName("Meeting_Code")]
    public string MeetingCode { get; set; }

    [JsonPropertyName("MeetingCountryKey")]
    public string MeetingCountryKey { get; set; }

    [JsonPropertyName("CircuitKey")]
    public string CircuitKey { get; set; }

    [JsonPropertyName("Meeting_Location")]
    public string MeetingLocation { get; set; }

    [JsonPropertyName("Series")]
    public string Series { get; set; }

    [JsonPropertyName("OBC")]
    public bool OBC { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("TimetableKey")]
    public string TimetableKey { get; set; }

    [JsonPropertyName("SessionKey")]
    public string SessionKey { get; set; }

    [JsonPropertyName("SessionPeriod")]
    public string SessionPeriod { get; set; }

    [JsonPropertyName("Circuit_Official_Name")]
    public string CircuitOfficialName { get; set; }

    [JsonPropertyName("ActivityDescription")]
    public string ActivityDescription { get; set; }

    [JsonPropertyName("SeriesMeetingSessionIdentifier")]
    public string SeriesMeetingSessionIdentifier { get; set; }

    [JsonPropertyName("sessionEndTime")]
    public string SessionEndTime { get; set; }

    [JsonPropertyName("Meeting_Start_Date")]
    public DateTime? MeetingStartDate { get; set; }

    [JsonPropertyName("Meeting_End_Date")]
    public DateTime? MeetingEndDate { get; set; }

    [JsonPropertyName("Track_Length")]
    public string TrackLength { get; set; }

    [JsonPropertyName("Scheduled_Lap_Count")]
    public string ScheduledLapCount { get; set; }

    [JsonPropertyName("Scheduled_Distance")]
    public string ScheduledDistance { get; set; }

    [JsonPropertyName("Circuit_Location")]
    public string CircuitLocation { get; set; }

    [JsonPropertyName("Meeting_Sponsor")]
    public string MeetingSponsor { get; set; }

    [JsonPropertyName("IsTestEvent")]
    public string IsTestEvent { get; set; }

    [JsonPropertyName("Season_Meeting_Ordinal")]
    public int SeasonMeetingOrdinal { get; set; }

    [JsonPropertyName("Championship_Meeting_Ordinal")]
    public string ChampionshipMeetingOrdinal { get; set; }

    [JsonPropertyName("session_index")]
    public int SessionIndex { get; set; }

    [JsonPropertyName("Meeting_Official_Name")]
    public string MeetingOfficialName { get; set; }

    [JsonPropertyName("Meeting_Display_Date")]
    public string MeetingDisplayDate { get; set; }

    [JsonPropertyName("PageID")]
    public int PageID { get; set; }

    [JsonPropertyName("Meeting_Country_Name")]
    public string MeetingCountryName { get; set; }

    [JsonPropertyName("sessionEndDate")]
    public long SessionEndDate { get; set; }

    [JsonPropertyName("sessionStartDate")]
    public long SessionStartDate { get; set; }

    [JsonPropertyName("Global_Title")]
    public string GlobalTitle { get; set; }

    [JsonPropertyName("Global_Meeting_Country_Name")]
    public string GlobalMeetingCountryName { get; set; }

    [JsonPropertyName("Global_Meeting_Name")]
    public string GlobalMeetingName { get; set; }
}