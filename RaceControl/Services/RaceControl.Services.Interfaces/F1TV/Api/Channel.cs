using Newtonsoft.Json;
using RaceControl.Common.Utils;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Channel
    {
        public const string WIF = "WIF";
        public const string PitLane = "pit lane";
        public const string Driver = "driver";
        public const string Data = "data";

        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public static string UIDField => JsonUtils.GetJsonPropertyName<Channel>(s => s.UID);
        public static string SelfField => JsonUtils.GetJsonPropertyName<Channel>(s => s.Self);
        public static string NameField => JsonUtils.GetJsonPropertyName<Channel>(s => s.Name);

        public override string ToString()
        {
            switch (Name)
            {
                case WIF:
                    return "World Feed";

                case PitLane:
                    return "Pit Lane";

                case Driver:
                    return "Driver Tracker";

                case Data:
                    return "Data";
            }

            return Name;
        }
    }
}