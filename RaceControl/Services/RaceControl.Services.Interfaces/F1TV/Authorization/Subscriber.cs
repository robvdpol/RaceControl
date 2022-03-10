namespace RaceControl.Services.Interfaces.F1TV.Authorization
{
    public class Subscriber
    {
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        [JsonPropertyName("HomeCountry")]
        public string HomeCountry { get; set; }

        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("Login")]
        public string Login { get; set; }
    }
}