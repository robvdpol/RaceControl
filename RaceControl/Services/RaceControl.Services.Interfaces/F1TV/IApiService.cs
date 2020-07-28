using RaceControl.Services.Interfaces.F1TV.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.F1TV
{
    public interface IApiService
    {
        Task<List<Season>> GetRaceSeasonsAsync();

        Task<Event> GetEventAsync(string uid);

        Task<Session> GetSessionAsync(string uid);
    }
}