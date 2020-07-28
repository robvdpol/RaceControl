using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceControl.Services.F1TV.Authorization
{
    public class TokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("oauth2_access_token")]
        public string OAuth2AccessToken { get; set; }

        [JsonProperty("plan_urls")]
        public List<string> PlanUrls { get; set; }

        [JsonProperty("user_is_vip")]
        public bool UserIsVip { get; set; }
    }
}