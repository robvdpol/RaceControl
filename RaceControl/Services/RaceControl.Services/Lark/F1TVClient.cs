using Newtonsoft.Json;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Lark;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace RaceControl.Services.Lark
{
    public class F1TVClient : LarkClient, IF1TVClient
    {
        public F1TVClient(Func<IRestClient> restClientFactory) : base(restClientFactory, Constants.ApiEndpointUrl)
        {
        }

        public async Task<TokenisedUrl> GetTokenisedUrlForChannelAsync(string token, string channelUrl)
        {
            var restClient = RestClientFactory();
            restClient.Authenticator = new F1TVAuthenticator(token);
            var restRequest = new RestRequest($"{Endpoint}/viewings").AddJsonBody(new ChannelUrl { Url = channelUrl });

            return await restClient.PostAsync<TokenisedUrl>(restRequest);
        }

        public async Task<TokenisedUrlContainer> GetTokenisedUrlForAssetAsync(string token, string assetUrl)
        {
            var restClient = RestClientFactory();
            restClient.Authenticator = new F1TVAuthenticator(token);
            var restRequest = new RestRequest($"{Endpoint}/viewings").AddJsonBody(new AssetUrl { Url = assetUrl });

            return await restClient.PostAsync<TokenisedUrlContainer>(restRequest);
        }

        public async Task<BackupStream> GetBackupStream()
        {
            var restClient = RestClientFactory();
            var restRequest = new RestRequest(Constants.BackupStreamUrl, DataFormat.Json);
            var restResponse = await restClient.ExecuteGetAsync(restRequest);

            if (restResponse.IsSuccessful)
            {
                return JsonConvert.DeserializeObject<BackupStream>(restResponse.Content);
            }
            else
            {
                throw new Exception($"Could not retrieve backup stream URL (HTTP status code {(int)restResponse.StatusCode}).", restResponse.ErrorException);
            }
        }
    }
}