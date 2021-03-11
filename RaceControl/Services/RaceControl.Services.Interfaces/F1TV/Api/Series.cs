namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Series
    {
        public string UID { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}