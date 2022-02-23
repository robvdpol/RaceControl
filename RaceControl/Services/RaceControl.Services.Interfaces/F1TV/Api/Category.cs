namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Category
    {
        [JsonProperty("categoryPathIds")]
        public List<int> CategoryPathIds { get; set; }

        [JsonProperty("externalPathIds")]
        public List<string> ExternalPathIds { get; set; }

        [JsonProperty("endDate")]
        public long EndDate { get; set; }

        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        [JsonProperty("isPrimary")]
        public bool IsPrimary { get; set; }

        [JsonProperty("categoryName")]
        public string CategoryName { get; set; }

        [JsonProperty("categoryId")]
        public int CategoryId { get; set; }

        [JsonProperty("startDate")]
        public long StartDate { get; set; }
    }
}