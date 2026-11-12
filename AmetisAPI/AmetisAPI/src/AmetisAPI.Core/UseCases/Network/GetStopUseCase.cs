using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Network
{
    internal class GetStopUseCase(ILogger<GetStopUseCase> p_Logger, IUnitOfWork p_UnitOfWork, INetworkRepository p_Network)
        : ADbUseCase<GetStopRequest, ValueResponse<StopDetail>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<StopDetail>> HandleLogic(GetStopRequest p_Request, CancellationToken p_CancellationToken)
        {
            StopDetail v_Stop = await WithDatabaseAsync(() => p_Network.GetStopAsync(p_Request.StopId, p_CancellationToken), p_CancellationToken);
            return v_Stop is null ? ValueResponse<StopDetail>.Error(DomainErrors.Data.NotFound, "Arrêt non trouvé") : new(v_Stop);
        }
    }
}
