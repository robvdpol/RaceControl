namespace RaceControl.Services.Interfaces.F1TV.Api;

public class RetrieveItems
{
    [JsonPropertyName("resultObj")]
    public ResultObj ResultObj { get; set; }

    [JsonPropertyName("uriOriginal")]
    public string UriOriginal { get; set; }

    [JsonPropertyName("typeOriginal")]
    public string TypeOriginal { get; set; }
}