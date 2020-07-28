using Newtonsoft.Json;

namespace RaceControl.Services.F1TV.Authorization
{
    public class AuthRequest
    {
        [JsonProperty("Login")]
        public string Login { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }
    }
}