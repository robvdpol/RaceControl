using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class TokenisedUrlContainer
    {
        [JsonProperty("objects")]
        public List<TokenisedUrlObject> Objects { get; set; }
    }
}