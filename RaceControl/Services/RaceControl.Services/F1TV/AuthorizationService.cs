namespace RaceControl.Services.F1TV;

public class AuthorizationService : IAuthorizationService
{
    private readonly ILogger _logger;
    private readonly RestClient _restClient;

    public AuthorizationService(ILogger logger, RestClient restClient)
    {
        _logger = logger;
        _restClient = restClient;
        _restClient.AddDefaultHeader("apiKey", Constants.ApiKey);
    }

    public async Task<AuthResponse> AuthenticateAsync(string login, string password)
    {
        var authRequest = new AuthRequest
        {
            Login = login,
            Password = password
        };

        _logger.Info($"Sending authorization request for login '{authRequest.Login}'...");
        var restRequest = new RestRequest(Constants.AuthenticateUrl, Method.Post).AddJsonBody(authRequest);
        var restResponse = await _restClient.ExecutePostAsync<AuthResponse>(restRequest);

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
