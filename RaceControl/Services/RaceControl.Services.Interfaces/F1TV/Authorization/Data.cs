namespace RaceControl.Services.Interfaces.F1TV.Authorization;

public class Data
{
    [JsonProperty("subscriptionStatus")]
    public string SubscriptionStatus { get; set; }

    [JsonProperty("subscriptionToken")]
    public string SubscriptionToken { get; set; }
}
