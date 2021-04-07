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

        public async Task<AuthResponse> AuthenticateAsync(string login, string password)
        {
            var authRequest = new AuthRequest
            {
                Login = login,
                Password = password
            };

            _logger.Info($"Sending authorization request for login '{authRequest.Login}'...");
            var restClient = _restClientFactory();
            var restRequest = new RestRequest(Constants.AuthenticateUrl).AddJsonBody(authRequest).AddHeader("apiKey", Constants.ApiKey);
            var restResponse = await restClient.ExecutePostAsync<AuthResponse>(restRequest);

            if (restResponse.IsSuccessful)
            {
                return restResponse.Data;
            }

            if (restResponse.ErrorException != null)
            {
                throw restResponse.ErrorException;
            }

            throw new Exception($"{(int)restResponse.StatusCode} - {restResponse.StatusDescription}");
        }
    }
}