using Newtonsoft.Json;

namespace RaceControl.Services.F1TV.Authorization
{
    public class AuthResponse
    {
        [JsonProperty("SessionId")]
        public string SessionId { get; set; }

        [JsonProperty("PasswordIsTemporary")]
        public bool PasswordIsTemporary { get; set; }

        [JsonProperty("Subscriber")]
        public Subscriber Subscriber { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}