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
                throw new Exception("An active F1TV subscription is required.");
            }
#endif

            var tokenRequest = new TokenRequest
            {
                AccessToken = authResponse.Data.SubscriptionToken,
                IdentityProviderUrl = Constants.IdentityProvider
            };

            _logger.Info("Sending token request...");
            var restClient = _restClientFactory();
            var restRequest = new RestRequest(Constants.TokenUrl).AddJsonBody(tokenRequest);

            return await restClient.PostAsync<TokenResponse>(restRequest);
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
            var restRequest = new RestRequest(Constants.AuthenticateUrl).AddJsonBody(authRequest).AddHeader("apiKey", Constants.ApiKey);

            return await restClient.PostAsync<AuthResponse>(restRequest);
        }
    }
}