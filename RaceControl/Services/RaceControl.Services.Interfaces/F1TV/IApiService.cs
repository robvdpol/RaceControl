using RaceControl.Services.Interfaces.F1TV.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.F1TV
{
    public interface IApiService
    {
        Task<List<Session>> GetLiveSessionsAsync();

        Task<List<Season>> GetSeasonsAsync();

        Task<List<Series>> GetSeriesAsync();

        Task<List<VodType>> GetVodTypesAsync();

        Task<List<Event>> GetEventsForSeasonAsync(string seasonUID);

        Task<List<Session>> GetSessionsForEventAsync(string eventUID);

        Task<List<Channel>> GetChannelsForSessionAsync(string sessionUID);

        Task<List<Episode>> GetEpisodesForSessionAsync(string sessionUID);

        Task<Episode> GetEpisodeAsync(string episodeUID);

        Task<string> GetTokenisedUrlForChannelAsync(string token, string channelUrl);

        Task<string> GetTokenisedUrlForAssetAsync(string token, string assetUrl);

        Task<string> GetTokenisedUrlAsync(string token, ContentType contentType, string contentUrl);
    }
}