using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class ResultObj
    {
        [JsonProperty("entitlementToken")]
        public string EntitlementToken { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("streamType")]
        public string StreamType { get; set; }

        [JsonProperty("containers")]
        public List<Container> Containers { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}