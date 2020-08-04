using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class AssetUrl
    {
        [JsonProperty("asset_url")]
        public string Url { get; set; }
    }
}