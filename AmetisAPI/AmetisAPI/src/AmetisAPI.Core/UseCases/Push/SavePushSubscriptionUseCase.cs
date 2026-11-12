using AmetisAPI.Core.Requests.Push;
using AmetisAPI.Domain.Bo.Push;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Push;

namespace AmetisAPI.Core.UseCases.Push
{
    internal class SavePushSubscriptionUseCase(ILogger<SavePushSubscriptionUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IPushSubscriptionRepository p_Subscriptions, IPushSender p_Push, IClock p_Clock)
        : ADbUseCase<SavePushSubscriptionRequest, ValueResponse<bool>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<bool>> HandleLogic(SavePushSubscriptionRequest p_Request, CancellationToken p_CancellationToken)
        {
            if (!p_Push.IsEnabled)
                return ValueResponse<bool>.Error(DomainErrors.Push.Disabled, "Notifications push non configurées sur ce serveur");
            await WithDatabaseAsync(async () =>
            {
                await p_Subscriptions.SaveAsync(p_Request.UserId, new PushSubscriptionInfo(p_Request.Endpoint, p_Request.P256dh, p_Request.Auth), p_Request.AllLines, p_Clock.UtcNow, p_CancellationToken);
                return true;
            }, p_CancellationToken);
            return new(true);
        }
    }
}
