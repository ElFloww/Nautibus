using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases.Accounts
{
    internal class ListFavoritesUseCase(ILogger<ListFavoritesUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IFavoriteRepository p_Favorites)
        : ADbUseCase<ListFavoritesRequest, ValueResponse<IReadOnlyList<Favorite>>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<IReadOnlyList<Favorite>>> HandleLogic(ListFavoritesRequest p_Request, CancellationToken p_CancellationToken)
        {
            return new(await WithDatabaseAsync(() => p_Favorites.ListAsync(p_Request.UserId, p_CancellationToken), p_CancellationToken));
        }
    }
}
