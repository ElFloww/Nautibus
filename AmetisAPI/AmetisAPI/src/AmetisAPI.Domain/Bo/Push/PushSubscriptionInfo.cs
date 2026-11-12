namespace AmetisAPI.Domain.Bo.Push
{
    /// <summary>Abonnement Web Push fourni par le navigateur (adresse du service push et clés de chiffrement).</summary>
    public record PushSubscriptionInfo(string Endpoint, string P256dh, string Auth);
}
