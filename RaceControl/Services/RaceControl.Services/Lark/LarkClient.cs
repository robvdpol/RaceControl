using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.Lark;
using System.Threading.Tasks;

namespace RaceControl.Services.Lark
{
    public class LarkClient : ILarkClient
    {
        protected readonly IRestClient _restClient;
        protected readonly string _endpoint;

        public LarkClient(IRestClient restClient, string endpoint)
        {
            _restClient = restClient;
            _endpoint = endpoint;
        }

        public ILarkRequest NewRequest(string collection, string id = null)
        {
            return new LarkRequest(_endpoint, collection, id);
        }

        public async Task<TResponse> GetItemAsync<TResponse>(ILarkRequest request)
        {
            var url = request.GetURL();

            return await _restClient.GetAsJsonAsync<TResponse>(url);
        }

        public async Task<ILarkCollection<TResponse>> GetCollectionAsync<TResponse>(ILarkRequest request)
        {
            var url = request.GetURL();

            return await _restClient.GetAsJsonAsync<LarkCollection<TResponse>>(url);
        }
    }
}