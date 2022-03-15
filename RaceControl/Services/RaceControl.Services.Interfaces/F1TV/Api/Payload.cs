namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Payload
{
    [JsonPropertyName("objectSubtype")]
    public string ObjectSubtype { get; set; }

    [JsonPropertyName("contentId")]
    public string ContentId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("objectType")]
    public string ObjectType { get; set; }
}