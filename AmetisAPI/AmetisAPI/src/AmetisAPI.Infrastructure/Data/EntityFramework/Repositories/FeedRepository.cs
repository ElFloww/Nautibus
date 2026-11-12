using System.Data;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Interfaces.Gtfs;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Écriture versionnée des GTFS. Volume (≈ 240 000 horaires) : insertion par SqlBulkCopy.
    /// Les lignes d'une version ne sont lues qu'une fois la version active.
    /// </summary>
    internal class FeedRepository(AppDbContext p_Context) : IFeedRepository
    {
        private static readonly string[] m_ContentTables = ["gtfs.StopTime", "gtfs.StopRoute", "gtfs.Trip", "gtfs.Stop", "gtfs.Route", "gtfs.Calendar", "gtfs.CalendarDate", "gtfs.ShapePoint"];

        /// <summary>Distance maximale de rattachement d'un arrêt temps réel à un arrêt du réseau.</summary>
        public const int MaxStopMatchMeters = 50;

        public async Task<FeedVersionInfo> GetAsync(int p_VersionId, CancellationToken p_CancellationToken)
        {
            FeedVersionEntity v_Entity = await p_Context.FeedVersions.AsNoTracking().SingleOrDefaultAsync(p_V => p_V.Id == p_VersionId, p_CancellationToken);
            return ToInfo(v_Entity);
        }

        public async Task<FeedVersionInfo> GetActiveAsync(FeedSource p_Source, CancellationToken p_CancellationToken)
        {
            FeedVersionEntity v_Entity = await p_Context.FeedVersions.AsNoTracking()
                .SingleOrDefaultAsync(p_V => p_V.Source == (byte)p_Source && p_V.IsActive, p_CancellationToken);
            return ToInfo(v_Entity);
        }

        public async Task<IReadOnlyList<FeedVersionInfo>> ListRecentAsync(int p_Count, CancellationToken p_CancellationToken)
        {
            List<FeedVersionEntity> v_Entities = await p_Context.FeedVersions.AsNoTracking().OrderByDescending(p_V => p_V.Id).Take(p_Count).ToListAsync(p_CancellationToken);
            return v_Entities.Select(ToInfo).ToList();
        }

        public async Task<int> StartVersionAsync(FeedSource p_Source, string p_SourceUrl, string p_ContentHash, CancellationToken p_CancellationToken)
        {
            FeedVersionEntity v_Entity = new()
            {
                Source = (byte)p_Source,
                Status = (byte)FeedImportStatus.Running,
                SourceUrl = p_SourceUrl,
                ContentHash = p_ContentHash,
                StartedAtUtc = DateTime.UtcNow
            };
            p_Context.FeedVersions.Add(v_Entity);
            await p_Context.SaveChangesAsync(p_CancellationToken);
            p_Context.ChangeTracker.Clear();
            return v_Entity.Id;
        }

        public Task MarkSkippedAsync(int p_VersionId, CancellationToken p_CancellationToken)
        {
            return p_Context.FeedVersions.Where(p_V => p_V.Id == p_VersionId).ExecuteUpdateAsync(p_S => p_S
                .SetProperty(p_V => p_V.Status, (byte)FeedImportStatus.Skipped)
                .SetProperty(p_V => p_V.FinishedAtUtc, DateTime.UtcNow), p_CancellationToken);
        }

        public async Task WriteContentAsync(int p_VersionId, GtfsFeed p_Feed, CancellationToken p_CancellationToken)
        {
            SqlConnection v_Connection = (SqlConnection)p_Context.Database.GetDbConnection();
            if (v_Connection.State != ConnectionState.Open)
                await v_Connection.OpenAsync(p_CancellationToken);

            await BulkAsync(v_Connection, "gtfs.Route", p_Feed.Routes, p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("RouteId", typeof(string), p_R => p_R.RouteId), ("ShortName", typeof(string), p_R => p_R.ShortName),
                ("LongName", typeof(string), p_R => p_R.LongName), ("RouteType", typeof(int), p_R => p_R.RouteType), ("Color", typeof(string), p_R => p_R.Color),
                ("TextColor", typeof(string), p_R => p_R.TextColor), ("SortOrder", typeof(int), p_R => p_R.SortOrder));
            await BulkAsync(v_Connection, "gtfs.Stop", p_Feed.Stops, p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("StopId", typeof(string), p_S => p_S.StopId), ("Code", typeof(string), p_S => p_S.Code),
                ("Name", typeof(string), p_S => p_S.Name), ("Latitude", typeof(double), p_S => p_S.Latitude), ("Longitude", typeof(double), p_S => p_S.Longitude),
                ("LocationType", typeof(int), p_S => p_S.LocationType), ("ParentStationId", typeof(string), p_S => p_S.ParentStationId),
                ("WheelchairBoarding", typeof(int), p_S => p_S.WheelchairBoarding));
            await BulkAsync(v_Connection, "gtfs.Trip", p_Feed.Trips, p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("TripId", typeof(string), p_T => p_T.TripId), ("RouteId", typeof(string), p_T => p_T.RouteId),
                ("ServiceId", typeof(string), p_T => p_T.ServiceId), ("Headsign", typeof(string), p_T => p_T.Headsign), ("DirectionId", typeof(int), p_T => p_T.DirectionId),
                ("ShapeId", typeof(string), p_T => p_T.ShapeId));
            await BulkAsync(v_Connection, "gtfs.StopTime", p_Feed.StopTimes, p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("TripId", typeof(string), p_S => p_S.TripId), ("StopSequence", typeof(int), p_S => p_S.StopSequence),
                ("StopId", typeof(string), p_S => p_S.StopId), ("ArrivalSeconds", typeof(int), p_S => p_S.ArrivalSeconds), ("DepartureSeconds", typeof(int), p_S => p_S.DepartureSeconds),
                ("PickupType", typeof(int), p_S => p_S.PickupType), ("DropOffType", typeof(int), p_S => p_S.DropOffType), ("ShapeDistTraveled", typeof(double), p_S => p_S.ShapeDistTraveled));
            await BulkAsync(v_Connection, "gtfs.Calendar", p_Feed.Calendars, p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("ServiceId", typeof(string), p_C => p_C.ServiceId), ("DaysMask", typeof(int), p_C => p_C.DaysMask),
                ("StartDate", typeof(DateTime), p_C => p_C.StartDate.ToDateTime(TimeOnly.MinValue)), ("EndDate", typeof(DateTime), p_C => p_C.EndDate.ToDateTime(TimeOnly.MinValue)));
            await BulkAsync(v_Connection, "gtfs.CalendarDate", DistinctCalendarDates(p_Feed), p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("ServiceId", typeof(string), p_D => p_D.ServiceId),
                ("Date", typeof(DateTime), p_D => p_D.Date.ToDateTime(TimeOnly.MinValue)), ("ExceptionType", typeof(int), p_D => p_D.ExceptionType));
            await BulkAsync(v_Connection, "gtfs.ShapePoint", p_Feed.ShapePoints.DistinctBy(p_P => (p_P.ShapeId, p_P.Sequence)), p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("ShapeId", typeof(string), p_P => p_P.ShapeId), ("Sequence", typeof(int), p_P => p_P.Sequence),
                ("Latitude", typeof(double), p_P => p_P.Latitude), ("Longitude", typeof(double), p_P => p_P.Longitude));
            await BulkAsync(v_Connection, "gtfs.ImportRejection", p_Feed.Rejections, p_CancellationToken,
                ("FeedVersionId", typeof(int), _ => p_VersionId), ("FileName", typeof(string), p_R => p_R.FileName),
                ("LineNumber", typeof(int), p_R => p_R.LineNumber), ("Reason", typeof(string), p_R => p_R.Reason));

            // Données dérivées : position géographique, destination des courses, lignes par arrêt, compteurs.
            await p_Context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE gtfs.Stop SET Location = geography::Point(Latitude, Longitude, 4326) WHERE FeedVersionId = {p_VersionId};
UPDATE t SET Destination = s.Name
FROM gtfs.Trip t
CROSS APPLY (SELECT TOP 1 st.StopId FROM gtfs.StopTime st WHERE st.FeedVersionId = t.FeedVersionId AND st.TripId = t.TripId ORDER BY st.StopSequence DESC) l
JOIN gtfs.Stop s ON s.FeedVersionId = t.FeedVersionId AND s.StopId = l.StopId
WHERE t.FeedVersionId = {p_VersionId};
INSERT INTO gtfs.StopRoute (FeedVersionId, StopId, RouteId)
SELECT DISTINCT st.FeedVersionId, st.StopId, t.RouteId
FROM gtfs.StopTime st JOIN gtfs.Trip t ON t.FeedVersionId = st.FeedVersionId AND t.TripId = st.TripId
WHERE st.FeedVersionId = {p_VersionId};", p_CancellationToken);

            DateOnly? v_From = p_Feed.Calendars.Select(p_C => (DateOnly?)p_C.StartDate).Concat(p_Feed.CalendarDates.Select(p_D => (DateOnly?)p_D.Date)).Min();
            DateOnly? v_To = p_Feed.Calendars.Select(p_C => (DateOnly?)p_C.EndDate).Concat(p_Feed.CalendarDates.Select(p_D => (DateOnly?)p_D.Date)).Max();
            await p_Context.FeedVersions.Where(p_V => p_V.Id == p_VersionId).ExecuteUpdateAsync(p_S => p_S
                .SetProperty(p_V => p_V.RouteCount, p_Feed.Routes.Count)
                .SetProperty(p_V => p_V.StopCount, p_Feed.Stops.Count)
                .SetProperty(p_V => p_V.TripCount, p_Feed.Trips.Count)
                .SetProperty(p_V => p_V.StopTimeCount, p_Feed.StopTimes.Count)
                .SetProperty(p_V => p_V.ShapePointCount, p_Feed.ShapePoints.Count)
                .SetProperty(p_V => p_V.RejectedCount, p_Feed.Rejections.Count)
                .SetProperty(p_V => p_V.ValidFrom, v_From)
                .SetProperty(p_V => p_V.ValidTo, v_To), p_CancellationToken);
        }

        public async Task ActivateAsync(int p_VersionId, FeedSource p_Source, string p_ChangeSummary, CancellationToken p_CancellationToken)
        {
            // Stratégie de réexécution SQL Server : la transaction doit être portée par la stratégie.
            await p_Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction v_Transaction = await p_Context.Database.BeginTransactionAsync(p_CancellationToken);
                await p_Context.FeedVersions.Where(p_V => p_V.Source == (byte)p_Source && p_V.IsActive)
                    .ExecuteUpdateAsync(p_S => p_S.SetProperty(p_V => p_V.IsActive, false), p_CancellationToken);
                await p_Context.FeedVersions.Where(p_V => p_V.Id == p_VersionId).ExecuteUpdateAsync(p_S => p_S
                    .SetProperty(p_V => p_V.IsActive, true)
                    .SetProperty(p_V => p_V.Status, (byte)FeedImportStatus.Succeeded)
                    .SetProperty(p_V => p_V.FinishedAtUtc, DateTime.UtcNow)
                    .SetProperty(p_V => p_V.ChangeSummary, p_ChangeSummary), p_CancellationToken);
                await v_Transaction.CommitAsync(p_CancellationToken);
            });

            // Purge des contenus des versions inactives de cette source (le journal FeedVersion est conservé).
            foreach (string v_Table in m_ContentTables)
            {
#pragma warning disable EF1002 // nom de table issu d'une liste constante, pas d'une saisie
                await p_Context.Database.ExecuteSqlRawAsync(
                    $"DELETE c FROM {v_Table} c JOIN gtfs.FeedVersion v ON v.Id = c.FeedVersionId WHERE v.Source = {{0}} AND v.IsActive = 0 AND v.Status <> {{1}}",
                    [(int)(byte)p_Source, (int)(byte)FeedImportStatus.Running], p_CancellationToken);
#pragma warning restore EF1002
            }
        }

        public async Task FailAsync(int p_VersionId, string p_ErrorMessage, CancellationToken p_CancellationToken)
        {
            await DeleteContentAsync(p_VersionId, p_CancellationToken);
            await p_Context.FeedVersions.Where(p_V => p_V.Id == p_VersionId).ExecuteUpdateAsync(p_S => p_S
                .SetProperty(p_V => p_V.Status, (byte)FeedImportStatus.Failed)
                .SetProperty(p_V => p_V.IsActive, false)
                .SetProperty(p_V => p_V.FinishedAtUtc, DateTime.UtcNow)
                .SetProperty(p_V => p_V.ErrorMessage, p_ErrorMessage), p_CancellationToken);
        }

        public async Task<int> RebuildMatchesAsync(CancellationToken p_CancellationToken)
        {
            FeedVersionInfo v_Network = await GetActiveAsync(FeedSource.Network, p_CancellationToken);
            FeedVersionInfo v_Realtime = await GetActiveAsync(FeedSource.Realtime, p_CancellationToken);
            if (v_Network is null || v_Realtime is null)
                return 0;

            // Confiance : 3 = < 15 m et même nom ; 2 = < 15 m ; 1 = < 50 m. Lignes : 3 = même nom court ;
            // 2 = nom court temps réel préfixé « NA » (NAM1 -> M1, hypothèse de l'audit P0 à confirmer).
            await p_Context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM gtfs.StopMatch;
DELETE FROM gtfs.RouteMatch;
INSERT INTO gtfs.StopMatch (RealtimeStopId, NetworkStopId, DistanceMeters, Confidence)
SELECT r.StopId, n.StopId, n.Distance,
       CASE WHEN n.Distance <= 15 AND n.Name = r.Name COLLATE French_CI_AI THEN 3 WHEN n.Distance <= 15 THEN 2 ELSE 1 END
FROM gtfs.Stop r
CROSS APPLY (SELECT TOP 1 s.StopId, s.Name, s.Location.STDistance(r.Location) AS Distance
             FROM gtfs.Stop s
             WHERE s.FeedVersionId = {v_Network.Id} AND s.LocationType = 0 AND s.Location.STDistance(r.Location) <= {MaxStopMatchMeters}
             ORDER BY s.Location.STDistance(r.Location)) n
WHERE r.FeedVersionId = {v_Realtime.Id} AND r.LocationType = 0;
INSERT INTO gtfs.RouteMatch (RealtimeRouteId, NetworkRouteId, Confidence)
SELECT r.RouteId, n.RouteId, CASE WHEN n.Exact = 1 THEN 3 ELSE 2 END
FROM gtfs.Route r
CROSS APPLY (SELECT TOP 1 x.RouteId, CASE WHEN x.ShortName = r.ShortName THEN 1 ELSE 0 END AS Exact
             FROM gtfs.Route x
             WHERE x.FeedVersionId = {v_Network.Id} AND (x.ShortName = r.ShortName OR 'NA' + x.ShortName = r.ShortName)
             ORDER BY CASE WHEN x.ShortName = r.ShortName THEN 0 ELSE 1 END) n
WHERE r.FeedVersionId = {v_Realtime.Id};", p_CancellationToken);
            return await p_Context.StopMatches.CountAsync(p_CancellationToken) + await p_Context.RouteMatches.CountAsync(p_CancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetRouteIdsAsync(int p_VersionId, CancellationToken p_CancellationToken)
            => await p_Context.Routes.Where(p_R => p_R.FeedVersionId == p_VersionId).Select(p_R => p_R.RouteId).ToListAsync(p_CancellationToken);

        public async Task<IReadOnlyList<string>> GetStopIdsAsync(int p_VersionId, CancellationToken p_CancellationToken)
            => await p_Context.Stops.Where(p_S => p_S.FeedVersionId == p_VersionId).Select(p_S => p_S.StopId).ToListAsync(p_CancellationToken);

        private async Task DeleteContentAsync(int p_VersionId, CancellationToken p_CancellationToken)
        {
            foreach (string v_Table in m_ContentTables)
            {
#pragma warning disable EF1002 // nom de table issu d'une liste constante, pas d'une saisie
                await p_Context.Database.ExecuteSqlRawAsync($"DELETE FROM {v_Table} WHERE FeedVersionId = {{0}}", [p_VersionId], p_CancellationToken);
#pragma warning restore EF1002
            }
        }

        /// <summary>
        /// Une même date ne peut figurer qu'une fois par service (clé primaire). Les doublons exacts sont fusionnés ;
        /// des doublons contradictoires (ajout et retrait le même jour) rendent le flux incohérent.
        /// </summary>
        private static IEnumerable<GtfsCalendarDate> DistinctCalendarDates(GtfsFeed p_Feed)
        {
            foreach (IGrouping<(string, DateOnly), GtfsCalendarDate> v_Group in p_Feed.CalendarDates.GroupBy(p_D => (p_D.ServiceId, p_D.Date)))
            {
                if (v_Group.Select(p_D => p_D.ExceptionType).Distinct().Count() > 1)
                    throw new Domain.Exceptions.GtfsFormatException($"calendar_dates.txt : exceptions contradictoires pour {v_Group.Key.Item1} le {v_Group.Key.Item2:yyyy-MM-dd}");
                yield return v_Group.First();
            }
        }

        private static async Task BulkAsync<T>(SqlConnection p_Connection, string p_Table, IEnumerable<T> p_Rows, CancellationToken p_CancellationToken,
            params (string Name, Type Type, Func<T, object> Value)[] p_Columns)
        {
            using DataTable v_Table = new();
            foreach ((string v_Name, Type v_Type, _) in p_Columns)
                v_Table.Columns.Add(v_Name, v_Type).AllowDBNull = true;
            foreach (T v_Row in p_Rows)
                v_Table.Rows.Add(p_Columns.Select(p_C => p_C.Value(v_Row) ?? DBNull.Value).ToArray());
            if (v_Table.Rows.Count == 0)
                return;

            using SqlBulkCopy v_Bulk = new(p_Connection, SqlBulkCopyOptions.CheckConstraints, null)
            {
                DestinationTableName = p_Table,
                BatchSize = 10_000,
                BulkCopyTimeout = 600
            };
            foreach ((string v_Name, _, _) in p_Columns)
                v_Bulk.ColumnMappings.Add(v_Name, v_Name);
            await v_Bulk.WriteToServerAsync(v_Table, p_CancellationToken);
        }

        private static FeedVersionInfo ToInfo(FeedVersionEntity p_Entity) => p_Entity is null ? null : new FeedVersionInfo(
            p_Entity.Id, (FeedSource)p_Entity.Source, (FeedImportStatus)p_Entity.Status, p_Entity.IsActive, p_Entity.ContentHash,
            p_Entity.StartedAtUtc, p_Entity.FinishedAtUtc, p_Entity.RouteCount, p_Entity.StopCount, p_Entity.TripCount, p_Entity.StopTimeCount,
            p_Entity.RejectedCount, p_Entity.ValidFrom, p_Entity.ValidTo, p_Entity.ChangeSummary, p_Entity.ErrorMessage);
    }
}
