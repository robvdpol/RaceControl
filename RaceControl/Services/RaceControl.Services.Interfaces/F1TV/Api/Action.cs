namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Action
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("targetType")]
    public string TargetType { get; set; }

    [JsonPropertyName("href")]
    public string Href { get; set; }
}