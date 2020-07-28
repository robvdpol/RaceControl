using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class ChannelUrl
    {
        [JsonProperty("channel_url")]
        public string Url { get; set; }
    }
}