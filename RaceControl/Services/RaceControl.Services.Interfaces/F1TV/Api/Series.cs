namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Series
    {
        public string UID { get; init; }
        public string Name { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
}