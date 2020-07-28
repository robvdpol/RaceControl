using Newtonsoft.Json;

namespace RaceControl.Services.F1TV.Authorization
{
    public class TokenRequest
    {
        [JsonProperty("identity_provider_url")]
        public string IdentityProviderUrl { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}