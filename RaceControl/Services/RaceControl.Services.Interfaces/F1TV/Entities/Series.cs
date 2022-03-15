namespace RaceControl.Services.Interfaces.F1TV.Entities;

public class Series
{
    public string UID { get; init; }
    public string Name { get; init; }

    public override string ToString()
    {
        return Name;
    }
}