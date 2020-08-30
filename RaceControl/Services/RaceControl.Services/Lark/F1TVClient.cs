using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Lark;
using System.Threading.Tasks;

namespace RaceControl.Services.Lark
{
    public class F1TVClient : LarkClient, IF1TVClient
    {
        private const string BackupStreamUrl = @"https://f1tv.formula1.com/dr/stream.json";

        public F1TVClient(IRestClient restClient) : base(restClient, "https://f1tv.formula1.com/api")
        {
        }

        public async Task<TokenisedUrl> GetTokenisedUrlForChannelAsync(string token, string channelUrl)
        {
            var url = Endpoint + "/viewings";
            var request = new ChannelUrl { Url = channelUrl };
            var tokenisedUrl = await RestClient.PostAsJsonAsync<ChannelUrl, TokenisedUrl>(url, request, null, token);

            return tokenisedUrl;
        }

        public async Task<TokenisedUrlContainer> GetTokenisedUrlForAssetAsync(string token, string assetUrl)
        {
            var url = Endpoint + "/viewings";
            var request = new AssetUrl { Url = assetUrl };
            var tokenisedUrlContainer = await RestClient.PostAsJsonAsync<AssetUrl, TokenisedUrlContainer>(url, request, null, token);

            return tokenisedUrlContainer;
        }

        public async Task<BackupStream> GetBackupStream()
        {
            return await RestClient.GetAsJsonAsync<BackupStream>(BackupStreamUrl);
        }
    }
}