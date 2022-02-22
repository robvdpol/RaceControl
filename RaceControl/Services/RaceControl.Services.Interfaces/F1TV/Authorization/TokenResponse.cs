using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Authorization
{
    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("oauth2_access_token")]
        public string OAuth2AccessToken { get; set; }

        [JsonPropertyName("plan_urls")]
        public List<string> PlanUrls { get; set; }

        [JsonPropertyName("user_is_vip")]
        public bool UserIsVip { get; set; }
    }
}