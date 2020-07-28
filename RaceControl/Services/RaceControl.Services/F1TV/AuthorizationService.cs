using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.F1TV
{
    public class AuthorizationService : IAuthorizationService
    {
        private const string _authUrl = @"https://api.formula1.com/v2/account/subscriber/authenticate/by-password";
        private const string _tokenUrl = @"https://f1tv-api.formula1.com/agl/1.0/unk/en/all_devices/global/authenticate";
        private const string _apiKey = @"fCUCjWrKPu9ylJwRAv8BpGLEgiAuThx7";
        private const string _identityProvider = @"/api/identity-providers/iden_732298a17f9c458890a1877880d140f3/";

        private readonly IRestClient _restClient;

        public AuthorizationService(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<AuthResponse> AuthenticateAsync(string login, string password)
        {
            var authRequest = new AuthRequest
            {
                Login = login,
                Password = password
            };

            var headers = new Dictionary<string, string>
            {
                { "apiKey", _apiKey }
            };

            return await _restClient.PostAsJsonAsync<AuthRequest, AuthResponse>(_authUrl, authRequest, headers);
        }

        public async Task<TokenResponse> LoginAsync(string login, string password)
        {
            var authResponse = await AuthenticateAsync(login, password);

            var tokenRequest = new TokenRequest
            {
                AccessToken = authResponse.Data.SubscriptionToken,
                IdentityProviderUrl = _identityProvider
            };

            return await _restClient.PostAsJsonAsync<TokenRequest, TokenResponse>(_tokenUrl, tokenRequest);
        }
    }
}