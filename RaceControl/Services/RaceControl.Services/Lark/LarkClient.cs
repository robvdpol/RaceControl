using RaceControl.Services.Interfaces.Lark;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace RaceControl.Services.Lark
{
    public abstract class LarkClient : ILarkClient
    {
        protected readonly Func<IRestClient> RestClientFactory;
        protected readonly string Endpoint;

        protected LarkClient(Func<IRestClient> restClientFactory, string endpoint)
        {
            RestClientFactory = restClientFactory;
            Endpoint = endpoint;
        }

        public ILarkRequest NewRequest(string collection, string id = null)
        {
            return new LarkRequest(Endpoint, collection, id);
        }

        public async Task<TResponse> GetItemAsync<TResponse>(ILarkRequest request)
        {
            var restClient = RestClientFactory();
            var restRequest = new RestRequest(request.GetUrl(), DataFormat.Json);

            return await restClient.GetAsync<TResponse>(restRequest);
        }

        public async Task<ILarkCollection<TResponse>> GetCollectionAsync<TResponse>(ILarkRequest request)
        {
            var restClient = RestClientFactory();
            var restRequest = new RestRequest(request.GetUrl(), DataFormat.Json);

            return await restClient.GetAsync<LarkCollection<TResponse>>(restRequest);
        }
    }
}