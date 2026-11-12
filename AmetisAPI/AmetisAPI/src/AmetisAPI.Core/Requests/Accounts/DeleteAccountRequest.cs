namespace AmetisAPI.Core.Requests.Accounts
{
    public record DeleteAccountRequest(string UserId) : IQuery<ValueResponse<bool>>;
}
