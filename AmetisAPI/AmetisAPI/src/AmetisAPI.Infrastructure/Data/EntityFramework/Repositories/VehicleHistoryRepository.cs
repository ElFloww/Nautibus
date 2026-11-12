using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Realtime;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class VehicleHistoryRepository(AppDbContext p_Context) : IVehicleHistoryRepository
    {
        public async Task<int> AppendAsync(IReadOnlyCollection<VehiclePosition> p_Positions, CancellationToken p_CancellationToken)
        {
            if (p_Positions.Count == 0)
                return 0;
            // Une position par véhicule et par horodatage : une collecte qui revoit la même position n'ajoute rien.
            string v_Json = System.Text.Json.JsonSerializer.Serialize(p_Positions.DistinctBy(p_P => (p_P.VehicleId, p_P.TimestampUtc)));
            return await p_Context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO rt.VehiclePosition (VehicleId, TripId, RouteId, Latitude, Longitude, Bearing, Speed, PositionTimestampUtc)
SELECT j.VehicleId, j.TripId, j.RouteId, j.Latitude, j.Longitude, j.Bearing, j.SpeedMetersPerSecond, j.TimestampUtc
FROM OPENJSON({v_Json}) WITH (
    VehicleId nvarchar(255), TripId nvarchar(255), RouteId nvarchar(255), Latitude float, Longitude float,
    Bearing float, SpeedMetersPerSecond float, TimestampUtc datetime2) j
WHERE NOT EXISTS (SELECT 1 FROM rt.VehiclePosition v WHERE v.VehicleId = j.VehicleId AND v.PositionTimestampUtc = j.TimestampUtc);", p_CancellationToken);
        }

        public async Task<IReadOnlyList<double[]>> GetTripTraceAsync(string p_TripId, DateTime p_FromUtc, DateTime p_ToUtc, CancellationToken p_CancellationToken)
        {
            return await p_Context.VehiclePositions.AsNoTracking()
                .Where(p_V => p_V.TripId == p_TripId && p_V.PositionTimestampUtc >= p_FromUtc && p_V.PositionTimestampUtc <= p_ToUtc)
                .OrderBy(p_V => p_V.PositionTimestampUtc)
                .Select(p_V => new[] { p_V.Longitude, p_V.Latitude })
                .ToListAsync(p_CancellationToken);
        }

        public Task<int> PurgeOlderThanAsync(DateTime p_LimitUtc, CancellationToken p_CancellationToken)
        {
            return p_Context.VehiclePositions.Where(p_V => p_V.PositionTimestampUtc < p_LimitUtc).ExecuteDeleteAsync(p_CancellationToken);
        }
    }
}
