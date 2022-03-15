namespace RaceControl.Services.Interfaces.F1TV.Api;

public class AudioLanguage
{
    [JsonPropertyName("audioLanguageName")]
    public string AudioLanguageName { get; set; }

    [JsonPropertyName("audioId")]
    public string AudioId { get; set; }

    [JsonPropertyName("isPreferred")]
    public bool IsPreferred { get; set; }
}