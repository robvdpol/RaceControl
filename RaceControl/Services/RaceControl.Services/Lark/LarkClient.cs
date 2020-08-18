using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.Lark;
using System.Threading.Tasks;

namespace RaceControl.Services.Lark
{
    public abstract class LarkClient : ILarkClient
    {
        protected readonly IRestClient RestClient;
        protected readonly string Endpoint;

        protected LarkClient(IRestClient restClient, string endpoint)
        {
            RestClient = restClient;
            Endpoint = endpoint;
        }

        public ILarkRequest NewRequest(string collection, string id = null)
        {
            return new LarkRequest(Endpoint, collection, id);
        }

        public async Task<TResponse> GetItemAsync<TResponse>(ILarkRequest request)
        {
            var url = request.GetURL();

            return await RestClient.GetAsJsonAsync<TResponse>(url);
        }

        public async Task<ILarkCollection<TResponse>> GetCollectionAsync<TResponse>(ILarkRequest request)
        {
            var url = request.GetURL();

            return await RestClient.GetAsJsonAsync<LarkCollection<TResponse>>(url);
        }
    }
}