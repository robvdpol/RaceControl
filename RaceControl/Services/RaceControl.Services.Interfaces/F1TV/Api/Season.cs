namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Season
    {
        public int? Year { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}