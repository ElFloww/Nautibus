using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Realtime;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class RealtimeReferenceRepository(AppDbContext p_Context) : IRealtimeReferenceRepository
    {
        public async Task<IReadOnlyList<RealtimeLineMatch>> GetLineMatchesAsync(CancellationToken p_CancellationToken)
        {
            int v_Network = await ActiveAsync(FeedSource.Network, p_CancellationToken);
            return await (
                    from v_M in p_Context.RouteMatches
                    join v_R in p_Context.Routes on v_M.NetworkRouteId equals v_R.RouteId
                    where v_R.FeedVersionId == v_Network
                    select new RealtimeLineMatch(v_M.RealtimeRouteId,
                        new LineSummary(v_R.RouteId, v_R.ShortName, v_R.LongName, v_R.Color, v_R.TextColor, v_R.SortOrder),
                        (MatchConfidence)v_M.Confidence))
                .ToListAsync(p_CancellationToken);
        }

        public async Task<IReadOnlyList<RealtimeTripInfo>> GetTripsAsync(IReadOnlyCollection<string> p_TripIds, CancellationToken p_CancellationToken)
        {
            int v_Realtime = await ActiveAsync(FeedSource.Realtime, p_CancellationToken);
            List<string> v_Ids = [.. p_TripIds];
            return await p_Context.Trips.Where(p_T => p_T.FeedVersionId == v_Realtime && v_Ids.Contains(p_T.TripId))
                .Select(p_T => new RealtimeTripInfo(p_T.TripId, p_T.RouteId, p_T.Headsign ?? p_T.Destination, p_T.DirectionId))
                .ToListAsync(p_CancellationToken);
        }

        public async Task<IReadOnlyList<RealtimeStopInfo>> GetRealtimeStopsAsync(IReadOnlyCollection<string> p_StopIds, CancellationToken p_CancellationToken)
        {
            int v_Realtime = await ActiveAsync(FeedSource.Realtime, p_CancellationToken);
            List<string> v_Ids = [.. p_StopIds];
            return await p_Context.Stops.Where(p_S => p_S.FeedVersionId == v_Realtime && v_Ids.Contains(p_S.StopId))
                .Select(p_S => new RealtimeStopInfo(p_S.StopId, p_S.Name, p_S.Latitude, p_S.Longitude))
                .ToListAsync(p_CancellationToken);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetRealtimeStopNamesAsync(IReadOnlyCollection<string> p_StopIds, CancellationToken p_CancellationToken)
        {
            int v_Realtime = await ActiveAsync(FeedSource.Realtime, p_CancellationToken);
            List<string> v_Ids = [.. p_StopIds];
            return await p_Context.Stops.Where(p_S => p_S.FeedVersionId == v_Realtime && v_Ids.Contains(p_S.StopId))
                .ToDictionaryAsync(p_S => p_S.StopId, p_S => p_S.Name, p_CancellationToken);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetNetworkStopsForRealtimeStopsAsync(IReadOnlyCollection<string> p_RealtimeStopIds, CancellationToken p_CancellationToken)
        {
            List<string> v_Ids = [.. p_RealtimeStopIds];
            return await p_Context.StopMatches.Where(p_M => v_Ids.Contains(p_M.RealtimeStopId))
                .ToDictionaryAsync(p_M => p_M.RealtimeStopId, p_M => p_M.NetworkStopId, p_CancellationToken);
        }

        public async Task<IReadOnlyList<RealtimeStopMatch>> GetStopMatchesForNetworkStopsAsync(IReadOnlyCollection<string> p_NetworkStopIds, CancellationToken p_CancellationToken)
        {
            List<string> v_Ids = [.. p_NetworkStopIds];
            return await p_Context.StopMatches.Where(p_M => v_Ids.Contains(p_M.NetworkStopId))
                .Select(p_M => new RealtimeStopMatch(p_M.RealtimeStopId, p_M.NetworkStopId, p_M.DistanceMeters, (MatchConfidence)p_M.Confidence))
                .ToListAsync(p_CancellationToken);
        }

        private Task<int> ActiveAsync(FeedSource p_Source, CancellationToken p_CancellationToken)
        {
            return p_Context.FeedVersions.Where(p_V => p_V.Source == (byte)p_Source && p_V.IsActive).Select(p_V => p_V.Id).SingleOrDefaultAsync(p_CancellationToken);
        }
    }
}
