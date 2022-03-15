namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Container
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("layout")]
    public string Layout { get; set; }

    [JsonPropertyName("actions")]
    public List<Action> Actions { get; set; }

    [JsonPropertyName("properties")]
    public List<Property> Properties { get; set; }

    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }

    [JsonPropertyName("bundles")]
    public List<Bundle> Bundles { get; set; }

    [JsonPropertyName("categories")]
    public List<Category> Categories { get; set; }

    [JsonPropertyName("platformVariants")]
    public List<PlatformVariant> PlatformVariants { get; set; }

    [JsonPropertyName("retrieveItems")]
    public RetrieveItems RetrieveItems { get; set; }

    [JsonPropertyName("contentId")]
    public long ContentId { get; set; }

    [JsonPropertyName("suggest")]
    public Suggest Suggest { get; set; }

    [JsonPropertyName("platformName")]
    public string PlatformName { get; set; }
}