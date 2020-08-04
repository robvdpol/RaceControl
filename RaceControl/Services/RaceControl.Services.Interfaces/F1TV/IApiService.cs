using RaceControl.Services.Interfaces.F1TV.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.F1TV
{
    public interface IApiService
    {
        Task<List<Season>> GetRaceSeasonsAsync();

        Task<Event> GetEventAsync(string eventUID);

        Task<Session> GetSessionAsync(string sessionUID);

        Task<List<Channel>> GetChannelsAsync(string sessionUID);

        Task<List<VodType>> GetVodTypesAsync();

        Task<Episode> GetEpisodeAsync(string episodeUID);

        Task<string> GetTokenisedUrlForChannelAsync(string token, string channelUrl);

        Task<string> GetTokenisedUrlForAssetAsync(string token, string assetUrl);
    }
}