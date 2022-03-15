namespace RaceControl.Services.Interfaces.F1TV.Api;

public class AdditionalStream
{
    [JsonPropertyName("racingNumber")]
    public int RacingNumber { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("driverFirstName")]
    public string DriverFirstName { get; set; }

    [JsonPropertyName("driverLastName")]
    public string DriverLastName { get; set; }

    [JsonPropertyName("teamName")]
    public string TeamName { get; set; }

    [JsonPropertyName("constructorName")]
    public string ConstructorName { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("playbackUrl")]
    public string PlaybackUrl { get; set; }

    [JsonPropertyName("driverImg")]
    public string DriverImg { get; set; }

    [JsonPropertyName("teamImg")]
    public string TeamImg { get; set; }

    [JsonPropertyName("hex")]
    public string Hex { get; set; }
}