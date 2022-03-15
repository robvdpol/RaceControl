namespace RaceControl.Services.Interfaces.F1TV.Entities;

public class Event
{
    public string UID { get; init; }
    public string Name { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }

    public override string ToString()
    {
        return Name;
    }
}