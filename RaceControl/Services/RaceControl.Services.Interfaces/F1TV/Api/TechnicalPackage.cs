namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class TechnicalPackage
    {
        [JsonProperty("packageId")]
        public int PackageId { get; set; }

        [JsonProperty("packageName")]
        public string PackageName { get; set; }

        [JsonProperty("packageType")]
        public string PackageType { get; set; }
    }
}