namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Bundle
{
    [JsonProperty("bundleSubtype")]
    public string BundleSubtype { get; set; }

    [JsonProperty("isParent")]
    public bool IsParent { get; set; }

    [JsonProperty("orderId")]
    public int OrderId { get; set; }

    [JsonProperty("bundleId")]
    public int BundleId { get; set; }

    [JsonProperty("bundleType")]
    public string BundleType { get; set; }
}
