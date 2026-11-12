namespace AmetisAPI.Core.Requests.Push
{
    /// <summary>AllLines : notifier tous les évènements ; sinon seulement ceux des lignes favorites.</summary>
    public record SavePushSubscriptionRequest(string UserId, string Endpoint, string P256dh, string Auth, bool AllLines) : IQuery<ValueResponse<bool>>;
}
