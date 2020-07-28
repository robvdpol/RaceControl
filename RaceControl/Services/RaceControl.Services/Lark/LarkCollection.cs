using Newtonsoft.Json;
using RaceControl.Services.Interfaces.Lark;
using System.Collections.Generic;

namespace RaceControl.Services.Lark
{
    public class LarkCollection<TResponse> : ILarkCollection<TResponse>
    {
        [JsonProperty("objects")]
        public List<TResponse> Objects { get; set; }
    }
}