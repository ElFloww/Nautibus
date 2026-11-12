using AmetisAPI.Core.Requests.Push;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Push;

namespace AmetisAPI.Core.UseCases.Push
{
    internal class GetPushKeyUseCase(ILogger<GetPushKeyUseCase> p_Logger, IPushSender p_Push)
        : AUseCase<GetPushKeyRequest, ValueResponse<string>>(p_Logger)
    {
        protected override Task<ValueResponse<string>> HandleLogic(GetPushKeyRequest p_Request, CancellationToken p_CancellationToken)
        {
            return Task.FromResult(p_Push.IsEnabled
                ? new ValueResponse<string>(p_Push.PublicKey)
                : ValueResponse<string>.Error(DomainErrors.Push.Disabled, "Notifications push non configurées sur ce serveur"));
        }
    }
}
