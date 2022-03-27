namespace RaceControl.Services.Github;

public class GithubService : IGithubService
{
    private const string RaceControlLatestReleaseUrl = @"https://api.github.com/repos/robvdpol/RaceControl/releases/latest";

    private readonly ILogger _logger;
    private readonly RestClient _restClient;

    public GithubService(ILogger logger, RestClient restClient)
    {
        _logger = logger;
        _restClient = restClient;
    }

    public async Task<Release> GetLatestRelease()
    {
        _logger.Info("Getting latest release from GitHub...");
        var restRequest = new RestRequest(RaceControlLatestReleaseUrl);

        return await _restClient.GetAsync<Release>(restRequest).ConfigureAwait(false);
    }
}