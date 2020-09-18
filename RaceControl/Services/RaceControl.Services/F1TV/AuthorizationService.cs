using NLog;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Authorization;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace RaceControl.Services.F1TV
{
    public class AuthorizationService : IAuthorizationService
    {
        private const string AuthUrl = @"https://api.formula1.com/v2/account/subscriber/authenticate/by-password";
        private const string TokenUrl = @"https://f1tv-api.formula1.com/agl/1.0/unk/en/all_devices/global/authenticate";
        private const string ApiKey = @"fCUCjWrKPu9ylJwRAv8BpGLEgiAuThx7";
        private const string IdentityProvider = @"/api/identity-providers/iden_732298a17f9c458890a1877880d140f3/";

        private readonly ILogger _logger;
        private readonly Func<IRestClient> _restClientFactory;

        public AuthorizationService(ILogger logger, Func<IRestClient> restClientFactory)
        {
            _logger = logger;
            _restClientFactory = restClientFactory;
        }

        public async Task<TokenResponse> LoginAsync(string login, string password)
        {
            var authResponse = await AuthenticateAsync(login, password);

#if !DEBUG
            if (authResponse.Data.SubscriptionStatus != "active")
            {
                throw new System.Exception("An active F1TV subscription is required.");
            }
#endif

            var tokenRequest = new TokenRequest
            {
                AccessToken = authResponse.Data.SubscriptionToken,
                IdentityProviderUrl = IdentityProvider
            };

            _logger.Info("Sending token request...");
            var restClient = _restClientFactory();
            var restRequest = new RestRequest(TokenUrl).AddJsonBody(tokenRequest);
            var tokenResponse = await restClient.PostAsync<TokenResponse>(restRequest);
            _logger.Info("Received token response.");

            return tokenResponse;
        }

        private async Task<AuthResponse> AuthenticateAsync(string login, string password)
        {
            var authRequest = new AuthRequest
            {
                Login = login,
                Password = password
            };

            _logger.Info($"Sending authorization request for login '{authRequest.Login}'...");
            var restClient = _restClientFactory();
            var restRequest = new RestRequest(AuthUrl).AddJsonBody(authRequest).AddHeader("apiKey", ApiKey);
            var authResponse = await restClient.PostAsync<AuthResponse>(restRequest);
            _logger.Info("Received authorization response.");

            return authResponse;
        }
    }
}