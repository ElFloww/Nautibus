using AmetisAPI.Domain.Bo.Accounts;

namespace AmetisAPI.Core.Requests.Accounts
{
    public record ListFavoritesRequest(string UserId) : IQuery<ValueResponse<IReadOnlyList<Favorite>>>;
}
