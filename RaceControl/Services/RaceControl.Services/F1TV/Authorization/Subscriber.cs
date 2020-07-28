using Newtonsoft.Json;

namespace RaceControl.Services.F1TV.Authorization
{
    public class Subscriber
    {
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("HomeCountry")]
        public string HomeCountry { get; set; }

        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Login")]
        public string Login { get; set; }
    }
}