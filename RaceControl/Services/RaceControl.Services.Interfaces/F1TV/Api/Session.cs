namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Session
    {
        public string UID { get; init; }
        public int ContentID { get; init; }
        public string ContentType { get; init; } // VIDEO, BUNDLE
        public string ContentSubtype { get; init; } // LIVE, FEATURE, DOCUMENTARY, REPLAY, SHOW, PRESS CONFERENCE, ANALYSIS, HIGHLIGHTS, EXTENDED HIGHLIGHTS, MEETING
        public string ShortName { get; init; }
        public string LongName { get; init; }
        public string SeriesUID { get; init; }
        public string ThumbnailUrl { get; init; }

        public bool IsLive => ContentType == "LIVE";

        public override string ToString()
        {
            return IsLive ? $"{ShortName} (live)" : ShortName;
        }
    }
}