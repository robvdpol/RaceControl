using System;

namespace RaceControl.Services.Interfaces.F1TV.Entities
{
    public class Event
    {
        public string UID { get; init; }
        public string Name { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }

        public override string ToString()
        {
            //if (StartDate.HasValue && EndDate.HasValue && StartDate != EndDate)
            //{
            //    // End date is always one day late for some reason
            //    var endDate = EndDate.Value.AddDays(-1);

            //    return $"{Name} ({StartDate:dd/MM}-{endDate:dd/MM})";
            //}

            //if (StartDate.HasValue)
            //{
            //    return $"{Name} ({StartDate:dd/MM})";
            //}

            return Name;
        }
    }
}