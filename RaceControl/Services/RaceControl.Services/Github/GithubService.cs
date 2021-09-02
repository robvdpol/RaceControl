using NLog;
using RaceControl.Services.Interfaces.Github;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace RaceControl.Services.Github
{
    public class GithubService : IGithubService
    {
        private const string RaceControlLatestReleaseUrl = @"https://api.github.com/repos/robvdpol/RaceControl/releases/latest";

        private readonly ILogger _logger;
        private readonly Func<IRestClient> _restClientFactory;

        public GithubService(ILogger logger, Func<IRestClient> restClientFactory)
        {
            _logger = logger;
            _restClientFactory = restClientFactory;
        }

        public async Task<Release> GetLatestRelease()
        {
            _logger.Info("Getting latest release from GitHub...");
            var restClient = _restClientFactory();
            var restRequest = new RestRequest(RaceControlLatestReleaseUrl, DataFormat.Json);

            return await restClient.GetAsync<Release>(restRequest);
        }
    }
}