namespace RaceControl.Services.Interfaces.F1TV.Api;

public class TechnicalPackage
{
    [JsonPropertyName("packageId")]
    public int PackageId { get; set; }

    [JsonPropertyName("packageName")]
    public string PackageName { get; set; }

    [JsonPropertyName("packageType")]
    public string PackageType { get; set; }
}