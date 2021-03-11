using System;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Event
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}