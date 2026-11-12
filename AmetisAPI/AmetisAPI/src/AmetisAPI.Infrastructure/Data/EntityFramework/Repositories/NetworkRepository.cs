using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    /// <summary>Lecture du réseau sur la version active du GTFS complet. Aucune version active : résultats vides.</summary>
    internal class NetworkRepository(AppDbContext p_Context) : INetworkRepository
    {
        private const int Wgs84 = 4326;

        public async Task<IReadOnlyList<LineSummary>> SearchLinesAsync(string p_Query, CancellationToken p_CancellationToken)
        {
            int v_Version = await ActiveVersionAsync(p_CancellationToken);
            IQueryable<RouteEntity> v_Routes = p_Context.Routes.Where(p_R => p_R.FeedVersionId == v_Version);
            if (!string.IsNullOrEmpty(p_Query))
                v_Routes = v_Routes.Where(p_R => p_R.ShortName.Contains(p_Query) || p_R.LongName.Contains(p_Query));
            return await v_Routes.OrderBy(p_R => p_R.SortOrder).ThenBy(p_R => p_R.ShortName)
                .Select(p_R => new LineSummary(p_R.RouteId, p_R.ShortName, p_R.LongName, p_R.Color, p_R.TextColor, p_R.SortOrder))
                .ToListAsync(p_CancellationToken);
        }

        public async Task<LineDetail> GetLineAsync(string p_LineId, CancellationToken p_CancellationToken)
        {
            int v_Version = await ActiveVersionAsync(p_CancellationToken);
            LineSummary v_Line = await p_Context.Routes.Where(p_R => p_R.FeedVersionId == v_Version && p_R.RouteId == p_LineId)
                .Select(p_R => new LineSummary(p_R.RouteId, p_R.ShortName, p_R.LongName, p_R.Color, p_R.TextColor, p_R.SortOrder))
                .SingleOrDefaultAsync(p_CancellationToken);
            if (v_Line is null)
                return null;

            var v_Trips = await p_Context.Trips.Where(p_T => p_T.FeedVersionId == v_Version && p_T.RouteId == p_LineId)
                .Select(p_T => new
                {
                    p_T.TripId,
                    p_T.DirectionId,
                    p_T.ShapeId,
                    p_T.Destination,
                    StopCount = p_Context.StopTimes.Count(p_S => p_S.FeedVersionId == v_Version && p_S.TripId == p_T.TripId)
                })
                .ToListAsync(p_CancellationToken);

            List<LineDirection> v_Directions = [];
            foreach (var v_Group in v_Trips.GroupBy(p_T => p_T.DirectionId ?? 0).OrderBy(p_G => p_G.Key))
            {
                // Parcours type : tracé le plus fréquent, puis la course qui y dessert le plus d'arrêts.
                string v_Shape = v_Group.GroupBy(p_T => p_T.ShapeId).OrderByDescending(p_G => p_G.Count()).First().Key;
                var v_Reference = v_Group.Where(p_T => p_T.ShapeId == v_Shape).OrderByDescending(p_T => p_T.StopCount).First();

                List<StopSummary> v_Stops = await (
                        from v_St in p_Context.StopTimes
                        join v_S in p_Context.Stops on new { v_St.FeedVersionId, v_St.StopId } equals new { v_S.FeedVersionId, v_S.StopId }
                        join v_P in p_Context.Stops on new { v_S.FeedVersionId, StopId = v_S.ParentStationId } equals new { v_P.FeedVersionId, v_P.StopId } into v_Parents
                        from v_P in v_Parents.DefaultIfEmpty()
                        where v_St.FeedVersionId == v_Version && v_St.TripId == v_Reference.TripId
                        orderby v_St.StopSequence
                        select new StopSummary(v_P != null ? v_P.StopId : v_S.StopId, v_P != null ? v_P.Name : v_S.Name, v_S.Latitude, v_S.Longitude, new List<LineSummary>()))
                    .ToListAsync(p_CancellationToken);
                v_Directions.Add(new LineDirection(v_Group.Key, v_Reference.Destination, v_Stops));
            }
            return new LineDetail(v_Line, v_Directions);
        }

        public async Task<IReadOnlyList<LineShape>> GetLineShapesAsync(string p_LineId, CancellationToken p_CancellationToken)
        {
            int v_Version = await ActiveVersionAsync(p_CancellationToken);
            IQueryable<string> v_ShapeIds = p_Context.Trips
                .Where(p_T => p_T.FeedVersionId == v_Version && p_T.RouteId == p_LineId && p_T.ShapeId != null)
                .Select(p_T => p_T.ShapeId).Distinct();
            List<ShapePointEntity> v_Points = await p_Context.ShapePoints.AsNoTracking()
                .Where(p_P => p_P.FeedVersionId == v_Version && v_ShapeIds.Contains(p_P.ShapeId))
                .OrderBy(p_P => p_P.ShapeId).ThenBy(p_P => p_P.Sequence)
                .ToListAsync(p_CancellationToken);
            return v_Points.GroupBy(p_P => p_P.ShapeId)
                .Select(p_G => new LineShape(p_G.Key, p_G.Select(p_P => new[] { p_P.Longitude, p_P.Latitude }).ToList()))
                .ToList();
        }

        public async Task<IReadOnlyList<StopSummary>> SearchStopsAsync(string p_Query, double? p_Latitude, double? p_Longitude, int p_RadiusMeters, int p_Limit, CancellationToken p_CancellationToken)
        {
            int v_Version = await ActiveVersionAsync(p_CancellationToken);
            // Arrêt usager : zone d'arrêt (location_type 1) ou quai sans zone parente.
            IQueryable<StopEntity> v_Areas = p_Context.Stops.Where(p_S => p_S.FeedVersionId == v_Version
                && (p_S.LocationType == 1 || p_S.LocationType == 0 && p_S.ParentStationId == null));
            if (!string.IsNullOrEmpty(p_Query))
                v_Areas = v_Areas.Where(p_S => p_S.Name.Contains(p_Query));

            List<StopEntity> v_Found;
            if (p_Latitude is { } v_Lat && p_Longitude is { } v_Lon)
            {
                Point v_Here = new(v_Lon, v_Lat) { SRID = Wgs84 };
                v_Found = await v_Areas.Where(p_S => p_S.Location.IsWithinDistance(v_Here, p_RadiusMeters))
                    .OrderBy(p_S => p_S.Location.Distance(v_Here)).Take(p_Limit).AsNoTracking().ToListAsync(p_CancellationToken);
            }
            else
            {
                v_Found = await v_Areas.OrderBy(p_S => p_S.Name).Take(p_Limit).AsNoTracking().ToListAsync(p_CancellationToken);
            }
            return await WithLinesAsync(v_Version, v_Found, p_CancellationToken);
        }

        public async Task<StopDetail> GetStopAsync(string p_StopId, CancellationToken p_CancellationToken)
        {
            int v_Version = await ActiveVersionAsync(p_CancellationToken);
            StopEntity v_Stop = await p_Context.Stops.AsNoTracking().SingleOrDefaultAsync(p_S => p_S.FeedVersionId == v_Version && p_S.StopId == p_StopId, p_CancellationToken);
            if (v_Stop is null)
                return null;
            if (v_Stop.ParentStationId is not null)
                v_Stop = await p_Context.Stops.AsNoTracking().SingleAsync(p_S => p_S.FeedVersionId == v_Version && p_S.StopId == v_Stop.ParentStationId, p_CancellationToken);

            List<StopEntity> v_Quays = await p_Context.Stops.AsNoTracking()
                .Where(p_S => p_S.FeedVersionId == v_Version && p_S.ParentStationId == v_Stop.StopId)
                .OrderBy(p_S => p_S.StopId).ToListAsync(p_CancellationToken);

            IReadOnlyList<StopSummary> v_Area = await WithLinesAsync(v_Version, [v_Stop], p_CancellationToken);
            IReadOnlyList<StopSummary> v_QuaySummaries = await WithLinesAsync(v_Version, v_Quays, p_CancellationToken, p_ByArea: false);
            return new StopDetail(v_Area[0], v_QuaySummaries);
        }

        public async Task<ServiceCalendarData> GetServiceCalendarAsync(CancellationToken p_CancellationToken)
        {
            int v_Version = await ActiveVersionAsync(p_CancellationToken);
            List<GtfsCalendar> v_Calendars = await p_Context.Calendars.Where(p_C => p_C.FeedVersionId == v_Version)
                .Select(p_C => new GtfsCalendar(p_C.ServiceId, p_C.DaysMask, p_C.StartDate, p_C.EndDate)).ToListAsync(p_CancellationToken);
            List<GtfsCalendarDate> v_Dates = await p_Context.CalendarDates.Where(p_D => p_D.FeedVersionId == v_Version)
                .Select(p_D => new GtfsCalendarDate(p_D.ServiceId, p_D.Date, p_D.ExceptionType)).ToListAsync(p_CancellationToken);
            return new ServiceCalendarData(v_Calendars, v_Dates);
        }

        public async Task<IReadOnlyList<ScheduledStopTime>> GetStopTimesAsync(IReadOnlyCollection<string> p_StopIds, IReadOnlyCollection<string> p_ServiceIds, int p_FromSeconds, int p_ToSeconds, CancellationToken p_CancellationToken)
        {
            int v_Version = await ActiveVersionAsync(p_CancellationToken);
            List<string> v_StopIds = [.. p_StopIds];
            List<string> v_ServiceIds = [.. p_ServiceIds];
            return await (
                    from v_St in p_Context.StopTimes
                    join v_T in p_Context.Trips on new { v_St.FeedVersionId, v_St.TripId } equals new { v_T.FeedVersionId, v_T.TripId }
                    join v_R in p_Context.Routes on new { v_T.FeedVersionId, v_T.RouteId } equals new { v_R.FeedVersionId, v_R.RouteId }
                    where v_St.FeedVersionId == v_Version
                          && v_StopIds.Contains(v_St.StopId)
                          && v_St.DepartureSeconds >= p_FromSeconds && v_St.DepartureSeconds <= p_ToSeconds
                          && v_St.PickupType != 1
                          && v_ServiceIds.Contains(v_T.ServiceId)
                    select new ScheduledStopTime(v_T.TripId, v_T.ServiceId, v_St.StopId, v_St.DepartureSeconds.Value, v_T.DirectionId, v_T.Destination,
                        new LineSummary(v_R.RouteId, v_R.ShortName, v_R.LongName, v_R.Color, v_R.TextColor, v_R.SortOrder)))
                .ToListAsync(p_CancellationToken);
        }

        /// <summary>Ajoute les lignes desservant chaque arrêt (par zone : lignes de tous ses quais).</summary>
        private async Task<IReadOnlyList<StopSummary>> WithLinesAsync(int p_Version, List<StopEntity> p_Stops, CancellationToken p_CancellationToken, bool p_ByArea = true)
        {
            List<string> v_Ids = p_Stops.Select(p_S => p_S.StopId).ToList();
            var v_Pairs = await (
                    from v_Sr in p_Context.StopRoutes
                    join v_S in p_Context.Stops on new { v_Sr.FeedVersionId, v_Sr.StopId } equals new { v_S.FeedVersionId, v_S.StopId }
                    join v_R in p_Context.Routes on new { v_Sr.FeedVersionId, v_Sr.RouteId } equals new { v_R.FeedVersionId, v_R.RouteId }
                    let v_Key = p_ByArea ? v_S.ParentStationId ?? v_S.StopId : v_S.StopId
                    where v_Sr.FeedVersionId == p_Version && v_Ids.Contains(v_Key)
                    select new { Key = v_Key, Line = new LineSummary(v_R.RouteId, v_R.ShortName, v_R.LongName, v_R.Color, v_R.TextColor, v_R.SortOrder) })
                .ToListAsync(p_CancellationToken);
            ILookup<string, LineSummary> v_Lines = v_Pairs.ToLookup(p_P => p_P.Key, p_P => p_P.Line);

            return p_Stops.Select(p_S => new StopSummary(p_S.StopId, p_S.Name, p_S.Latitude, p_S.Longitude,
                    v_Lines[p_S.StopId].DistinctBy(p_L => p_L.Id).OrderBy(p_L => p_L.SortOrder).ThenBy(p_L => p_L.ShortName).ToList()))
                .ToList();
        }

        private async Task<int> ActiveVersionAsync(CancellationToken p_CancellationToken)
        {
            return await p_Context.FeedVersions.Where(p_V => p_V.Source == (byte)FeedSource.Network && p_V.IsActive)
                .Select(p_V => p_V.Id).SingleOrDefaultAsync(p_CancellationToken);
        }
    }
}
