namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Bundle
{
    [JsonPropertyName("bundleSubtype")]
    public string BundleSubtype { get; set; }

    [JsonPropertyName("isParent")]
    public bool IsParent { get; set; }

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("bundleId")]
    public int BundleId { get; set; }

    [JsonPropertyName("bundleType")]
    public string BundleType { get; set; }
}