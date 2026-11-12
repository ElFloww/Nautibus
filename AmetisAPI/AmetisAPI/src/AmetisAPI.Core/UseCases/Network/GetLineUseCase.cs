using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Network
{
    internal class GetLineUseCase(ILogger<GetLineUseCase> p_Logger, IUnitOfWork p_UnitOfWork, INetworkRepository p_Network)
        : ADbUseCase<GetLineRequest, ValueResponse<LineDetail>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<LineDetail>> HandleLogic(GetLineRequest p_Request, CancellationToken p_CancellationToken)
        {
            LineDetail v_Line = await WithDatabaseAsync(() => p_Network.GetLineAsync(p_Request.LineId, p_CancellationToken), p_CancellationToken);
            return v_Line is null ? ValueResponse<LineDetail>.Error(DomainErrors.Data.NotFound, "Ligne non trouvée") : new(v_Line);
        }
    }
}
