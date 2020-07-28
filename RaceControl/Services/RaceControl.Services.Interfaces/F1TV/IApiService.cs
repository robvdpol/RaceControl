using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Lark;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.F1TV
{
    public interface IApiService
    {
        Task<ILarkCollection<Season>> GetRaceSeasonsAsync();

        Task<Event> GetEventAsync(string uid);
    }
}