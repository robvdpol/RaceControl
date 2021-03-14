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
            var startDate = StartDate?.ToString("dd\\/MM");
            var endDate = EndDate?.ToString("dd\\/MM");

            var hasDate = StartDate.HasValue || EndDate.HasValue;
            var hasBothDatesAndDifferent = hasDate && StartDate != EndDate;

            var dateString = !hasDate ? null : $" ({startDate}{(hasBothDatesAndDifferent ? "-" : "")}{endDate})";

            return Name + dateString;
        }
    }
}