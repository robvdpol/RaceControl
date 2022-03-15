namespace RaceControl.Services.Interfaces.F1TV.Authorization;

public class Data
{
    [JsonPropertyName("subscriptionStatus")]
    public string SubscriptionStatus { get; set; }

    [JsonPropertyName("subscriptionToken")]
    public string SubscriptionToken { get; set; }
}
