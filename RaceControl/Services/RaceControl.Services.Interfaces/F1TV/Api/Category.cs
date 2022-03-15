namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Category
{
    [JsonPropertyName("categoryPathIds")]
    public List<int> CategoryPathIds { get; set; }

    [JsonPropertyName("externalPathIds")]
    public List<string> ExternalPathIds { get; set; }

    [JsonPropertyName("endDate")]
    public long EndDate { get; set; }

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; }

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; }

    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [JsonPropertyName("startDate")]
    public long StartDate { get; set; }
}