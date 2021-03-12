namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Session
    {
        public string UID { get; set; }
        public int ContentID { get; set; }
        public string ContentType { get; set; } // VIDEO, BUNDLE
        public string ContentSubtype { get; set; } // LIVE, FEATURE, DOCUMENTARY, REPLAY, SHOW, PRESS CONFERENCE, ANALYSIS, HIGHLIGHTS, EXTENDED HIGHLIGHTS, MEETING
        public string Name { get; set; }
        public string SessionName { get; set; }
        public string SeriesUID { get; set; }
        public string ThumbnailUrl { get; set; }

        public bool IsLive => ContentType == "LIVE";

        public override string ToString()
        {
            return IsLive ? $"{Name} (live)" : Name;
        }
    }
}