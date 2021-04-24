using RaceControl.Common.Interfaces;
using RaceControl.Services.Interfaces.F1TV.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.F1TV
{
    public interface IApiService
    {
        List<Series> GetSeries();

        List<Season> GetSeasons();

        Task<List<Session>> GetLiveSessionsAsync();

        Task<List<Event>> GetEventsForSeasonAsync(Season season);

        Task<List<Episode>> GetEpisodesForSeasonAsync(Season season);

        Task<List<Session>> GetSessionsForEventAsync(Event evt);

        Task<List<Episode>> GetEpisodesForEventAsync(Event evt);

        Task<List<Channel>> GetChannelsForSessionAsync(Session session);

        Task<List<string>> GetVodGenresAsync();

        Task<List<Episode>> GetEpisodesForGenreAsync(string genre);

        Task<string> GetTokenisedUrlAsync(string subscriptionToken, IPlayableContent playableContent);
    }
}