using System.Text.Json.Serialization;

namespace RaceControl.Services.Interfaces.F1TV.Authorization
{
    public class AuthRequest
    {
        [JsonPropertyName("Login")]
        public string Login { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }
    }
}