namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Property
{
    [JsonProperty("meeting_Number")]
    public int MeetingNumber { get; set; }

    [JsonProperty("sessionEndTime")]
    public long SessionEndTime { get; set; }

    [JsonProperty("series")]
    public string Series { get; set; }

    [JsonProperty("lastUpdatedDate")]
    public long LastUpdatedDate { get; set; }

    [JsonProperty("season_Meeting_Ordinal")]
    public int SeasonMeetingOrdinal { get; set; }

    [JsonProperty("meeting_Start_Date")]
    public long MeetingStartDate { get; set; }

    [JsonProperty("meeting_End_Date")]
    public long MeetingEndDate { get; set; }

    [JsonProperty("season")]
    public int Season { get; set; }

    [JsonProperty("session_index")]
    public int SessionIndex { get; set; }

    [JsonProperty("meetingSessionKey")]
    public int MeetingSessionKey { get; set; }

    [JsonProperty("sessionStartDate")]
    public long SessionStartDate { get; set; }

    [JsonProperty("sessionEndDate")]
    public long SessionEndDate { get; set; }
}
