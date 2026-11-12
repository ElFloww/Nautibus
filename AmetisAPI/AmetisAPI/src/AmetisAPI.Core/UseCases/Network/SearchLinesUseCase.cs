using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Network
{
    internal class SearchLinesUseCase(ILogger<SearchLinesUseCase> p_Logger, IUnitOfWork p_UnitOfWork, INetworkRepository p_Network)
        : ADbUseCase<SearchLinesRequest, ValueResponse<IReadOnlyList<LineSummary>>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<IReadOnlyList<LineSummary>>> HandleLogic(SearchLinesRequest p_Request, CancellationToken p_CancellationToken)
        {
            return new(await WithDatabaseAsync(() => p_Network.SearchLinesAsync(p_Request.Query?.Trim(), p_CancellationToken), p_CancellationToken));
        }
    }
}
