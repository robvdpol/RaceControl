using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Suggest
    {
        [JsonPropertyName("input")]
        public List<string> Input { get; set; }

        [JsonPropertyName("payload")]
        public Payload Payload { get; set; }
    }
}