using RaceControl.Common.Interfaces;
using RaceControl.Services.Interfaces.F1TV.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.F1TV
{
    public interface IApiService
    {
        Task<List<Session>> GetLiveSessionsAsync();

        List<Season> GetSeasons();

        List<Series> GetSeries();

        Task<List<VodType>> GetVodTypesAsync();

        Task<List<Event>> GetEventsForSeasonAsync(Season season);

        Task<List<Session>> GetSessionsForEventAsync(Event evt);

        Task<List<Episode>> GetEpisodesForEventAsync(Event evt);

        Task<List<Channel>> GetChannelsForSessionAsync(Session session);

        Task<string> GetTokenisedUrlAsync(string token, IPlayableContent playableContent);
    }
}