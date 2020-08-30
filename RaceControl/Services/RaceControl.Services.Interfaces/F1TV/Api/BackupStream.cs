using Newtonsoft.Json;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class BackupStream
    {
        [JsonProperty("metricsEnvKeyPreProd")]
        public string MetricsEnvKeyPreProd { get; set; }

        [JsonProperty("metricsEnvKeyProd")]
        public string MetricsEnvKeyProd { get; set; }

        [JsonProperty("streamManifest")]
        public string StreamManifest { get; set; }

        [JsonProperty("poster")]
        public string Poster { get; set; }
    }
}