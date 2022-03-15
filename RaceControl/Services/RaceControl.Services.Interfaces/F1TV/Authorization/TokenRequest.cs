namespace RaceControl.Services.Interfaces.F1TV.Authorization;

public class TokenRequest
{
    [JsonPropertyName("identity_provider_url")]
    public string IdentityProviderUrl { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}