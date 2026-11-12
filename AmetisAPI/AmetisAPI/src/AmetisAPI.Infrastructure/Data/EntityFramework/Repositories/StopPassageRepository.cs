using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Realtime;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class StopPassageRepository(AppDbContext p_Context) : IStopPassageRepository
    {
        /// <summary>Une course ne dure pas plus longtemps : sert à isoler une circulation des autres jours.</summary>
        private static readonly TimeSpan m_MaxTripDuration = TimeSpan.FromHours(6);

        public async Task<int> UpsertAsync(IReadOnlyCollection<StopPassage> p_Passages, DateTime p_NowUtc, CancellationToken p_CancellationToken)
        {
            if (p_Passages.Count == 0)
                return 0;
            string v_Json = System.Text.Json.JsonSerializer.Serialize(p_Passages.DistinctBy(p_P => (p_P.TripId, p_P.StopId, p_P.ScheduledUtc)));
            return await p_Context.Database.ExecuteSqlInterpolatedAsync($@"
MERGE rt.StopPassage AS t
USING (SELECT * FROM OPENJSON({v_Json}) WITH (
    TripId nvarchar(255), RouteId nvarchar(255), StopId nvarchar(255), StopOrder int,
    ScheduledUtc datetime2, ExpectedUtc datetime2, DelaySeconds int, VehicleId nvarchar(255))) AS s
ON t.TripId = s.TripId AND t.StopId = s.StopId AND t.ScheduledUtc = s.ScheduledUtc
WHEN MATCHED AND (t.ExpectedUtc <> s.ExpectedUtc OR t.DelaySeconds <> s.DelaySeconds OR ISNULL(t.VehicleId, '') <> ISNULL(s.VehicleId, '') OR t.StopOrder <> s.StopOrder) THEN
    UPDATE SET ExpectedUtc = s.ExpectedUtc, DelaySeconds = s.DelaySeconds, VehicleId = s.VehicleId, StopOrder = s.StopOrder, UpdatedUtc = {p_NowUtc}
WHEN NOT MATCHED THEN
    INSERT (TripId, RouteId, StopId, StopOrder, ScheduledUtc, ExpectedUtc, DelaySeconds, VehicleId, UpdatedUtc)
    VALUES (s.TripId, s.RouteId, s.StopId, s.StopOrder, s.ScheduledUtc, s.ExpectedUtc, s.DelaySeconds, s.VehicleId, {p_NowUtc});", p_CancellationToken);
        }

        public async Task<IReadOnlyList<StopPassage>> ListForTripAsync(string p_TripId, DateTime p_NowUtc, CancellationToken p_CancellationToken)
        {
            // Même identifiant de course chaque jour : on retient la circulation la plus proche de maintenant.
            DateTime v_From = p_NowUtc.AddHours(-12), v_To = p_NowUtc.AddHours(12);
            List<Domain.Bo.Realtime.StopPassage> v_All = await p_Context.StopPassages.AsNoTracking()
                .Where(p_P => p_P.TripId == p_TripId && p_P.ScheduledUtc >= v_From && p_P.ScheduledUtc <= v_To)
                .Select(p_P => new StopPassage(p_P.TripId, p_P.RouteId, p_P.StopId, p_P.StopOrder, p_P.ScheduledUtc, p_P.ExpectedUtc, p_P.DelaySeconds, p_P.VehicleId, false))
                .ToListAsync(p_CancellationToken);
            if (v_All.Count == 0)
                return v_All;
            DateTime v_Pivot = v_All.MinBy(p_P => Math.Abs((p_P.ScheduledUtc - p_NowUtc).Ticks)).ScheduledUtc;
            return v_All.Where(p_P => Math.Abs((p_P.ScheduledUtc - v_Pivot).Ticks) <= m_MaxTripDuration.Ticks)
                .OrderBy(p_P => p_P.StopOrder).ThenBy(p_P => p_P.ScheduledUtc).ToList();
        }

        public async Task<IReadOnlyList<StopPassage>> ListForStopsAsync(IReadOnlyCollection<string> p_StopIds, DateTime p_FromUtc, DateTime p_ToUtc, CancellationToken p_CancellationToken)
        {
            List<string> v_Ids = [.. p_StopIds];
            return await p_Context.StopPassages.AsNoTracking()
                .Where(p_P => v_Ids.Contains(p_P.StopId) && p_P.ExpectedUtc >= p_FromUtc && p_P.ExpectedUtc <= p_ToUtc)
                .Select(p_P => new StopPassage(p_P.TripId, p_P.RouteId, p_P.StopId, p_P.StopOrder, p_P.ScheduledUtc, p_P.ExpectedUtc, p_P.DelaySeconds, p_P.VehicleId,
                    !p_Context.StopPassages.Any(p_O => p_O.TripId == p_P.TripId && p_O.StopOrder > p_P.StopOrder
                        && p_O.ScheduledUtc >= p_P.ScheduledUtc && p_O.ScheduledUtc <= p_P.ScheduledUtc.AddHours(6))))
                .ToListAsync(p_CancellationToken);
        }

        public Task<int> PurgeOlderThanAsync(DateTime p_LimitUtc, CancellationToken p_CancellationToken)
        {
            return p_Context.StopPassages.Where(p_P => p_P.ScheduledUtc < p_LimitUtc).ExecuteDeleteAsync(p_CancellationToken);
        }
    }
}
