using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Accounts
{
    /// <summary>Ajoute un favori après avoir vérifié que l'arrêt ou la ligne existe. Retourne false s'il existait déjà.</summary>
    internal class AddFavoriteUseCase(ILogger<AddFavoriteUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IFavoriteRepository p_Favorites, INetworkRepository p_Network, IClock p_Clock)
        : ADbUseCase<AddFavoriteRequest, ValueResponse<bool>>(p_Logger, p_UnitOfWork)
    {
        protected override Task<ValueResponse<bool>> HandleLogic(AddFavoriteRequest p_Request, CancellationToken p_CancellationToken)
        {
            return WithDatabaseAsync(async () =>
            {
                bool v_Exists = p_Request.Kind switch
                {
                    FavoriteKind.Stop => await p_Network.GetStopAsync(p_Request.TargetId, p_CancellationToken) is not null,
                    FavoriteKind.Line => await p_Network.GetLineAsync(p_Request.TargetId, p_CancellationToken) is not null,
                    _ => false
                };
                if (!v_Exists)
                    return ValueResponse<bool>.Error(DomainErrors.Data.NotFound, "Arrêt ou ligne inconnu");

                return new ValueResponse<bool>(await p_Favorites.AddAsync(p_Request.UserId, p_Request.Kind, p_Request.TargetId, p_Clock.UtcNow, p_CancellationToken));
            }, p_CancellationToken);
        }
    }
}
