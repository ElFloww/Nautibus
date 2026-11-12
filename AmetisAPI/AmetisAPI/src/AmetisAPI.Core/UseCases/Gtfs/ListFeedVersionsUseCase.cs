using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Gtfs;

namespace AmetisAPI.Core.UseCases.Gtfs
{
    internal class ListFeedVersionsUseCase(ILogger<ListFeedVersionsUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IFeedRepository p_Feeds)
        : ADbUseCase<ListFeedVersionsRequest, ValueResponse<IReadOnlyList<FeedVersionInfo>>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<IReadOnlyList<FeedVersionInfo>>> HandleLogic(ListFeedVersionsRequest p_Request, CancellationToken p_CancellationToken)
        {
            return new(await WithDatabaseAsync(() => p_Feeds.ListRecentAsync(p_Request.Count, p_CancellationToken), p_CancellationToken));
        }
    }
}
