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
        private const string BackupStreamUrl = @"https://f1tv.formula1.com/dr/stream.json";

        public F1TVClient(Func<IRestClient> restClientFactory) : base(restClientFactory, "https://f1tv.formula1.com/api")
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
            var restRequest = new RestRequest(BackupStreamUrl, DataFormat.Json);
            var restResponse = await restClient.ExecuteGetAsync(restRequest);

            return JsonConvert.DeserializeObject<BackupStream>(restResponse.Content);
        }
    }
}