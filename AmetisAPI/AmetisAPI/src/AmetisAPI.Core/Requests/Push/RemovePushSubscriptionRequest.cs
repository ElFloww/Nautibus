namespace AmetisAPI.Core.Requests.Push
{
    public record RemovePushSubscriptionRequest(string UserId, string Endpoint) : IQuery<ValueResponse<bool>>;
}
