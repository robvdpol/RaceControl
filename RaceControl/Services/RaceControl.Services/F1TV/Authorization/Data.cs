using Newtonsoft.Json;

namespace RaceControl.Services.F1TV.Authorization
{
    public class Data
    {
        [JsonProperty("subscriptionStatus")]
        public string SubscriptionStatus { get; set; }

        [JsonProperty("subscriptionToken")]
        public string SubscriptionToken { get; set; }
    }
}