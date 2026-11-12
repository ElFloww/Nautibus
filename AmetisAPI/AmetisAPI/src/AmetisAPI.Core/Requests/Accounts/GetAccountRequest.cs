using AmetisAPI.Domain.Bo.Accounts;

namespace AmetisAPI.Core.Requests.Accounts
{
    public record GetAccountRequest(string UserId) : IQuery<ValueResponse<AccountInfo>>;
}
