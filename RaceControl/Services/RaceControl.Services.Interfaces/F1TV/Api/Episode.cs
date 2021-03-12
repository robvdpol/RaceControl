namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Episode
    {
        public string UID { get; set; }
        public int ContentID { get; set; }
        public string ContentType { get; set; } // VIDEO, BUNDLE
        public string ContentSubtype { get; set; } // LIVE, FEATURE, DOCUMENTARY, REPLAY, SHOW, PRESS CONFERENCE, ANALYSIS, HIGHLIGHTS, EXTENDED HIGHLIGHTS, MEETING
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string SeriesUID { get; set; }
        public string PlaybackUrl { get; set; }
        public string ThumbnailUrl { get; set; }

        public override string ToString()
        {
            return ShortName;
        }
    }
}