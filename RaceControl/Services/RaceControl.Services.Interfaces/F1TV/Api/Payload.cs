namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Payload
    {
        [JsonProperty("objectSubtype")]
        public string ObjectSubtype { get; set; }

        [JsonProperty("contentId")]
        public string ContentId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("objectType")]
        public string ObjectType { get; set; }
    }
}