using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.Github
{
    public interface IGithubService
    {
        Task<Release> GetLatestRelease();
    }
}