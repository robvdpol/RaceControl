using Newtonsoft.Json;
using RaceControl.Common.Utils;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Series
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public bool IsChecked { get; set; } = true;

        public static string UIDField => JsonUtils.GetJsonPropertyName<Channel>(s => s.UID);
        public static string SelfField => JsonUtils.GetJsonPropertyName<Channel>(s => s.Self);
        public static string NameField => JsonUtils.GetJsonPropertyName<Channel>(s => s.Name);

        public override string ToString()
        {
            return Name;
        }
    }
}