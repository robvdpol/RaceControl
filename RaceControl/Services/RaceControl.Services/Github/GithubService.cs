using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.Github;
using System.Threading.Tasks;

namespace RaceControl.Services.Github
{
    public class GithubService : IGithubService
    {
        private const string RaceControlLatestReleaseUrl = @"https://api.github.com/repos/robvdpol/RaceControl/releases/latest";

        private readonly IRestClient _restClient;

        public GithubService(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<Release> GetLatestRelease()
        {
            return await _restClient.GetAsJsonAsync<Release>(RaceControlLatestReleaseUrl);
        }
    }
}