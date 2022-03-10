using System.Text.Json.Serialization;

namespace RaceControl.Services.Interfaces.F1TV.Authorization
{
    public class AuthResponse
    {
        [JsonPropertyName("SessionId")]
        public string SessionId { get; set; }

        [JsonPropertyName("PasswordIsTemporary")]
        public bool PasswordIsTemporary { get; set; }

        [JsonPropertyName("Subscriber")]
        public Subscriber Subscriber { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
}