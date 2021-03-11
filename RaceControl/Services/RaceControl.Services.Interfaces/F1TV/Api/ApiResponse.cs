using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class ApiResponse
    {
        [JsonProperty("resultCode")]
        public string ResultCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("errorDescription")]
        public string ErrorDescription { get; set; }

        [JsonProperty("resultObj")]
        public ResultObj ResultObj { get; set; }

        [JsonProperty("systemTime")]
        public long SystemTime { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}