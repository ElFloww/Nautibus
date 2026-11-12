using AmetisAPI.Domain.Bo.Accounts;

namespace AmetisAPI.Core.Requests.Accounts
{
    public record AddFavoriteRequest(string UserId, FavoriteKind Kind, string TargetId) : IQuery<ValueResponse<bool>>;
}
