namespace AmetisAPI.API.Controllers
{
    /// <summary>Abonnement transmis par le navigateur (PushSubscription.toJSON aplati).</summary>
    public record PushSubscriptionBody(string Endpoint, string P256dh, string Auth, bool AllLines);
}
