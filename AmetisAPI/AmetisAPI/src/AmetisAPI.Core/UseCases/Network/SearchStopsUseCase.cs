using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Network
{
    internal class SearchStopsUseCase(ILogger<SearchStopsUseCase> p_Logger, IUnitOfWork p_UnitOfWork, INetworkRepository p_Network)
        : ADbUseCase<SearchStopsRequest, ValueResponse<IReadOnlyList<StopSummary>>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<IReadOnlyList<StopSummary>>> HandleLogic(SearchStopsRequest p_Request, CancellationToken p_CancellationToken)
        {
            return new(await WithDatabaseAsync(
                () => p_Network.SearchStopsAsync(p_Request.Query?.Trim(), p_Request.Latitude, p_Request.Longitude, p_Request.RadiusMeters, p_Request.Limit, p_CancellationToken),
                p_CancellationToken));
        }
    }
}
