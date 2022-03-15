namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Property
{
    [JsonPropertyName("meeting_Number")]
    public int MeetingNumber { get; set; }

    [JsonPropertyName("sessionEndTime")]
    public long SessionEndTime { get; set; }

    [JsonPropertyName("series")]
    public string Series { get; set; }

    [JsonPropertyName("lastUpdatedDate")]
    public long LastUpdatedDate { get; set; }

    [JsonPropertyName("season_Meeting_Ordinal")]
    public int SeasonMeetingOrdinal { get; set; }

    [JsonPropertyName("meeting_Start_Date")]
    public long MeetingStartDate { get; set; }

    [JsonPropertyName("meeting_End_Date")]
    public long MeetingEndDate { get; set; }

    [JsonPropertyName("season")]
    public int Season { get; set; }

    [JsonPropertyName("session_index")]
    public int SessionIndex { get; set; }

    [JsonPropertyName("meetingSessionKey")]
    public int MeetingSessionKey { get; set; }

    [JsonPropertyName("sessionStartDate")]
    public long SessionStartDate { get; set; }

    [JsonPropertyName("sessionEndDate")]
    public long SessionEndDate { get; set; }
}