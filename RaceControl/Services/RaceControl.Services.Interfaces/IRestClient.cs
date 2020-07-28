using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces
{
    public interface IRestClient
    {
        Task<TResponse> GetAsJsonAsync<TResponse>(string url);

        Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest requestObject, IDictionary<string, string> requestHeaders = null);
    }
}