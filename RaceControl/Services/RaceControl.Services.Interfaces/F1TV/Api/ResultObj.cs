namespace RaceControl.Services.Interfaces.F1TV.Api;

public class ResultObj
{
    [JsonPropertyName("entitlementToken")]
    public string EntitlementToken { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("streamType")]
    public string StreamType { get; set; }

    [JsonPropertyName("containers")]
    public List<Container> Containers { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}