namespace RaceControl.Services.Interfaces.Github;

public class Asset
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; }
}