using AmetisAPI.Core.Gtfs;
using AmetisAPI.Core.Network;
using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Gtfs;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Gtfs
{
    internal class GetDataHealthUseCase(ILogger<GetDataHealthUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IFeedRepository p_Feeds, IRealtimeStore p_Store, IClock p_Clock)
        : ADbUseCase<GetDataHealthRequest, ValueResponse<DataHealth>>(p_Logger, p_UnitOfWork)
    {
        private const int InspectedVersions = 20;

        protected override async Task<ValueResponse<DataHealth>> HandleLogic(GetDataHealthRequest p_Request, CancellationToken p_CancellationToken)
        {
            IReadOnlyList<FeedVersionInfo> v_Versions = await WithDatabaseAsync(() => p_Feeds.ListRecentAsync(InspectedVersions, p_CancellationToken), p_CancellationToken);
            DateTime v_Now = p_Clock.UtcNow;
            return new(DataHealthEvaluator.Evaluate(
                v_Versions,
                RealtimeStatusFactory.From(await p_Store.GetAsync(p_CancellationToken), v_Now),
                DateOnly.FromDateTime(NetworkTime.ToParis(v_Now).DateTime)));
        }
    }
}
