using System;
using System.Globalization;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Session
    {
        public string UID { get; set; }
        public int ContentID { get; set; }
        public string Name { get; set; }
        public string SessionName { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public string SeriesUID { get; set; }
        public string ThumbnailUrl { get; set; }

        public bool IsUpcoming => Status == "upcoming";
        public bool IsLive => Status == "live";
        public bool IsExpired => Status == "expired";

        public override string ToString()
        {
            if (IsUpcoming)
            {
                var day = StartTime.ToString("ddd");
                var time = StartTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);

                return $"{Name} (starts {day} {time})";
            }

            if (IsLive)
            {
                return $"{Name} (live)";
            }

            if (IsExpired)
            {
                return $"{Name} (expired)";
            }

            return Name;
        }
    }
}