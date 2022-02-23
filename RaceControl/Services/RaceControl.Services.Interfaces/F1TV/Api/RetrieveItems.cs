namespace RaceControl.Services.Interfaces.F1TV.Api;

public class RetrieveItems
{
    [JsonProperty("resultObj")]
    public ResultObj ResultObj { get; set; }

    [JsonProperty("uriOriginal")]
    public string UriOriginal { get; set; }

    [JsonProperty("typeOriginal")]
    public string TypeOriginal { get; set; }
}
