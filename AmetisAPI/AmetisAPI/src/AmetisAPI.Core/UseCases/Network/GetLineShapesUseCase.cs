using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Network
{
    internal class GetLineShapesUseCase(ILogger<GetLineShapesUseCase> p_Logger, IUnitOfWork p_UnitOfWork, INetworkRepository p_Network)
        : ADbUseCase<GetLineShapesRequest, ValueResponse<IReadOnlyList<LineShape>>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<IReadOnlyList<LineShape>>> HandleLogic(GetLineShapesRequest p_Request, CancellationToken p_CancellationToken)
        {
            return new(await WithDatabaseAsync(() => p_Network.GetLineShapesAsync(p_Request.LineId, p_CancellationToken), p_CancellationToken));
        }
    }
}
