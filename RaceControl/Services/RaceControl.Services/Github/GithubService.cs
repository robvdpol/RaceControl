using NLog;
using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.Github;
using System.Threading.Tasks;

namespace RaceControl.Services.Github
{
    public class GithubService : IGithubService
    {
        private const string RaceControlLatestReleaseUrl = @"https://api.github.com/repos/robvdpol/RaceControl/releases/latest";

        private readonly ILogger _logger;
        private readonly IRestClient _restClient;

        public GithubService(ILogger logger, IRestClient restClient)
        {
            _logger = logger;
            _restClient = restClient;
        }

        public async Task<Release> GetLatestRelease()
        {
            _logger.Info("Getting latest release from Github...");
            var release = await _restClient.GetAsJsonAsync<Release>(RaceControlLatestReleaseUrl, nameof(RaceControl));
            _logger.Info($"Got release '{release.Name}' from Github.");

            return release;
        }
    }
}