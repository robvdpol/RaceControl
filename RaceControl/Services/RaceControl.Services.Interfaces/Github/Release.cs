namespace RaceControl.Services.Interfaces.Github;

public class Release
{
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }

    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool PreRelease { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; }

    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; }
}