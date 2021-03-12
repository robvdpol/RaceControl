namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Season
    {
        public int? Year { get; init; }
        public string Name { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
}