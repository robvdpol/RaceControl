using Newtonsoft.Json;
using RaceControl.Common;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Channel
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<Channel>((s) => s.UID);
        public static string SelfField => JsonUtils.GetJsonPropertyName<Channel>((s) => s.Self);
        public static string NameField => JsonUtils.GetJsonPropertyName<Channel>((s) => s.Name);

        public override string ToString()
        {
            switch (Name)
            {
                case "WIF":
                    return "World Feed";

                case "pit lane":
                    return "Pit Lane";

                case "driver":
                    return "Driver Tracker";

                case "data":
                    return "Data";
            }

            return Name;
        }
    }
}