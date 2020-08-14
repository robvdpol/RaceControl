using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces
{
    public interface IRestClient
    {
        Task<TResponse> GetAsJsonAsync<TResponse>(string url, string userAgent = null);

        Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest requestObject, IDictionary<string, string> requestHeaders = null, string token = null);
    }
}