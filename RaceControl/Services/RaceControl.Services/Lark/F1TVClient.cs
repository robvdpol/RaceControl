using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Lark;
using System.Threading.Tasks;

namespace RaceControl.Services.Lark
{
    public class F1TVClient : LarkClient, IF1TVClient
    {
        public F1TVClient(IRestClient restClient) : base(restClient, "https://f1tv.formula1.com/api")
        {
        }

        public async Task<TokenisedUrl> GetTokenisedUrlForChannelAsync(string token, string channelUrl)
        {
            var url = _endpoint + "/viewings";
            var request = new ChannelUrl { Url = channelUrl };
            var tokenisedUrl = await _restClient.PostAsJsonAsync<ChannelUrl, TokenisedUrl>(url, request, null, token);

            return tokenisedUrl;
        }

        public async Task<TokenisedUrlContainer> GetTokenisedUrlForAssetAsync(string token, string assetUrl)
        {
            var url = _endpoint + "/viewings";
            var request = new AssetUrl { Url = assetUrl };
            var tokenisedUrlContainer = await _restClient.PostAsJsonAsync<AssetUrl, TokenisedUrlContainer>(url, request, null, token);

            return tokenisedUrlContainer;
        }
    }
}