using Newtonsoft.Json;
using RaceControl.Common.Enum;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Utils;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Channel : IPlayable
    {
        [JsonProperty("uid")]
        public string UID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("channel_type")]
        public string ChannelType { get; set; }

        [JsonProperty("driveroccurrence_urls")]
        public List<string> DriverOccurrenceUrls { get; set; }

        [JsonIgnore]
        public ContentType ContentType => ChannelType == ChannelTypes.BACKUP ? ContentType.Backup : ContentType.Channel;

        [JsonIgnore]
        public string ContentUrl => Self;

        public static string UIDField => JsonUtils.GetJsonPropertyName<Channel>(c => c.UID);
        public static string SelfField => JsonUtils.GetJsonPropertyName<Channel>(c => c.Self);
        public static string NameField => JsonUtils.GetJsonPropertyName<Channel>(c => c.Name);
        public static string ChannelTypeField => JsonUtils.GetJsonPropertyName<Channel>(c => c.ChannelType);
        public static string DriverOccurrenceUrlsField => JsonUtils.GetJsonPropertyName<Channel>(c => c.DriverOccurrenceUrls);

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
                    return "Live Timing";

                default:
                    return Name;
            }
        }
    }
}