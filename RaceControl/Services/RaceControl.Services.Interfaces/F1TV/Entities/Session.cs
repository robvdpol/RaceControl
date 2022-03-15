namespace RaceControl.Services.Interfaces.F1TV.Entities;

public class Session
{
    public string UID { get; init; }
    public long ContentID { get; init; }
    public string ContentType { get; init; } // VIDEO, BUNDLE
    public string ContentSubtype { get; init; } // LIVE, FEATURE, DOCUMENTARY, REPLAY, SHOW, PRESS CONFERENCE, ANALYSIS, HIGHLIGHTS, EXTENDED HIGHLIGHTS, MEETING
    public string ShortName { get; init; }
    public string LongName { get; init; }
    public string SeriesUID { get; init; }
    public string ThumbnailUrl { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public long SessionIndex { get; init; }

    public bool IsLive => ContentSubtype == "LIVE";

    public override string ToString()
    {
        return IsLive ? $"{ShortName} (live)" : ShortName;
    }
}