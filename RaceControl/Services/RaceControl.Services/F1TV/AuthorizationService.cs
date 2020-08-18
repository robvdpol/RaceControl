using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.F1TV
{
    public class AuthorizationService : IAuthorizationService
    {
        private const string AuthUrl = @"https://api.formula1.com/v2/account/subscriber/authenticate/by-password";
        private const string TokenUrl = @"https://f1tv-api.formula1.com/agl/1.0/unk/en/all_devices/global/authenticate";
        private const string ApiKey = @"fCUCjWrKPu9ylJwRAv8BpGLEgiAuThx7";
        private const string IdentityProvider = @"/api/identity-providers/iden_732298a17f9c458890a1877880d140f3/";

        private readonly IRestClient _restClient;

        public AuthorizationService(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<TokenResponse> LoginAsync(string login, string password)
        {
            var authResponse = await AuthenticateAsync(login, password);

            if (authResponse.Data.SubscriptionStatus != "active")
            {
                throw new Exception("An active F1TV subscription is required.");
            }

            var tokenRequest = new TokenRequest
            {
                AccessToken = authResponse.Data.SubscriptionToken,
                IdentityProviderUrl = IdentityProvider
            };

            return await _restClient.PostAsJsonAsync<TokenRequest, TokenResponse>(TokenUrl, tokenRequest);
        }

        private async Task<AuthResponse> AuthenticateAsync(string login, string password)
        {
            var authRequest = new AuthRequest
            {
                Login = login,
                Password = password
            };

            var headers = new Dictionary<string, string>
            {
                { "apiKey", ApiKey }
            };

            return await _restClient.PostAsJsonAsync<AuthRequest, AuthResponse>(AuthUrl, authRequest, headers);
        }
    }
}