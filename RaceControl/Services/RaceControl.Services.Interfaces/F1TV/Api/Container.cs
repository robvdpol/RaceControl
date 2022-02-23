namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Container
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("layout")]
        public string Layout { get; set; }

        [JsonProperty("actions")]
        public List<Action> Actions { get; set; }

        [JsonProperty("properties")]
        public List<Property> Properties { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("bundles")]
        public List<Bundle> Bundles { get; set; }

        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }

        [JsonProperty("platformVariants")]
        public List<PlatformVariant> PlatformVariants { get; set; }

        [JsonProperty("retrieveItems")]
        public RetrieveItems RetrieveItems { get; set; }

        [JsonProperty("contentId")]
        public long ContentId { get; set; }

        [JsonProperty("suggest")]
        public Suggest Suggest { get; set; }

        [JsonProperty("platformName")]
        public string PlatformName { get; set; }
    }
}