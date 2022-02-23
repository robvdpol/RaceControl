namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class AdditionalStream
    {
        [JsonProperty("racingNumber")]
        public int RacingNumber { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("driverFirstName")]
        public string DriverFirstName { get; set; }

        [JsonProperty("driverLastName")]
        public string DriverLastName { get; set; }

        [JsonProperty("teamName")]
        public string TeamName { get; set; }

        [JsonProperty("constructorName")]
        public string ConstructorName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("playbackUrl")]
        public string PlaybackUrl { get; set; }

        [JsonProperty("driverImg")]
        public string DriverImg { get; set; }

        [JsonProperty("teamImg")]
        public string TeamImg { get; set; }

        [JsonProperty("hex")]
        public string Hex { get; set; }
    }
}