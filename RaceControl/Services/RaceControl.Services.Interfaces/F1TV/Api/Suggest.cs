namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Suggest
{
    [JsonProperty("input")]
    public List<string> Input { get; set; }

    [JsonProperty("payload")]
    public Payload Payload { get; set; }
}
