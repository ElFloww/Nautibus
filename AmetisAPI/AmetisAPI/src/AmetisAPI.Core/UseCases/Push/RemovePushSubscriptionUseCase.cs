using AmetisAPI.Core.Requests.Push;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Push;

namespace AmetisAPI.Core.UseCases.Push
{
    /// <summary>Désinscription : idempotente (renvoie false si l'abonnement n'existait plus).</summary>
    internal class RemovePushSubscriptionUseCase(ILogger<RemovePushSubscriptionUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IPushSubscriptionRepository p_Subscriptions)
        : ADbUseCase<RemovePushSubscriptionRequest, ValueResponse<bool>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<bool>> HandleLogic(RemovePushSubscriptionRequest p_Request, CancellationToken p_CancellationToken)
        {
            return new(await WithDatabaseAsync(() => p_Subscriptions.RemoveAsync(p_Request.UserId, p_Request.Endpoint, p_CancellationToken), p_CancellationToken));
        }
    }
}
