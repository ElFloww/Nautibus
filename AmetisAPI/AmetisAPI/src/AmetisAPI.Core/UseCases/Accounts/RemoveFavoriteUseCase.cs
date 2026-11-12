using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases.Accounts
{
    internal class RemoveFavoriteUseCase(ILogger<RemoveFavoriteUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IFavoriteRepository p_Favorites)
        : ADbUseCase<RemoveFavoriteRequest, ValueResponse<bool>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<bool>> HandleLogic(RemoveFavoriteRequest p_Request, CancellationToken p_CancellationToken)
        {
            bool v_Removed = await WithDatabaseAsync(() => p_Favorites.RemoveAsync(p_Request.UserId, p_Request.Kind, p_Request.TargetId, p_CancellationToken), p_CancellationToken);
            return v_Removed ? new(true) : ValueResponse<bool>.Error(DomainErrors.Data.NotFound, "Favori inexistant");
        }
    }
}
