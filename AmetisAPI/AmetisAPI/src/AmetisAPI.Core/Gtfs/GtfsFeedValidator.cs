using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Exceptions;

namespace AmetisAPI.Core.Gtfs
{
    /// <summary>
    /// Règles d'intégrité d'un GTFS lu : chaque enregistrement écarté est consigné avec sa raison.
    /// Un flux vide ou trop dégradé fait échouer l'import (les données actives restent en place).
    /// </summary>
    internal static class GtfsFeedValidator
    {
        /// <summary>Au-delà de cette part d'horaires rejetés, le flux est jugé incohérent.</summary>
        public const double MaxRejectedStopTimeRatio = 0.05;

        public static GtfsFeed Validate(GtfsFeed p_Feed)
        {
            List<GtfsRejection> v_Rejections = [.. p_Feed.Rejections];

            List<GtfsRoute> v_Routes = Distinct(p_Feed.Routes, p_R => p_R.RouteId, "routes.txt", "route_id", v_Rejections);
            HashSet<string> v_RouteIds = v_Routes.Select(p_R => p_R.RouteId).ToHashSet();

            List<GtfsStop> v_Stops = Distinct(p_Feed.Stops, p_S => p_S.StopId, "stops.txt", "stop_id", v_Rejections);
            HashSet<string> v_StopIds = v_Stops.Select(p_S => p_S.StopId).ToHashSet();
            v_Stops = v_Stops.Select(p_S =>
            {
                if (string.IsNullOrEmpty(p_S.ParentStationId) || v_StopIds.Contains(p_S.ParentStationId))
                    return p_S;
                v_Rejections.Add(new GtfsRejection("stops.txt", 0, $"stop_id={p_S.StopId} : parent_station {p_S.ParentStationId} inconnu, rattachement ignoré"));
                return p_S with { ParentStationId = null };
            }).ToList();

            List<GtfsCalendar> v_Calendars = Distinct(p_Feed.Calendars, p_C => p_C.ServiceId, "calendar.txt", "service_id", v_Rejections);
            HashSet<string> v_ServiceIds = v_Calendars.Select(p_C => p_C.ServiceId)
                .Concat(p_Feed.CalendarDates.Select(p_D => p_D.ServiceId))
                .ToHashSet();

            List<GtfsTrip> v_Trips = [];
            foreach (GtfsTrip v_Trip in Distinct(p_Feed.Trips, p_T => p_T.TripId, "trips.txt", "trip_id", v_Rejections))
            {
                if (!v_RouteIds.Contains(v_Trip.RouteId))
                    v_Rejections.Add(new GtfsRejection("trips.txt", 0, $"trip_id={v_Trip.TripId} : route_id {v_Trip.RouteId} inconnu"));
                else if (!v_ServiceIds.Contains(v_Trip.ServiceId))
                    v_Rejections.Add(new GtfsRejection("trips.txt", 0, $"trip_id={v_Trip.TripId} : service_id {v_Trip.ServiceId} absent du calendrier"));
                else
                    v_Trips.Add(v_Trip);
            }
            HashSet<string> v_TripIds = v_Trips.Select(p_T => p_T.TripId).ToHashSet();

            List<GtfsStopTime> v_StopTimes = [];
            HashSet<(string, int)> v_SeenStopTimes = [];
            int v_RejectedStopTimes = 0;
            foreach (GtfsStopTime v_StopTime in p_Feed.StopTimes)
            {
                string v_Reason = !v_TripIds.Contains(v_StopTime.TripId) ? $"trip_id {v_StopTime.TripId} inconnu ou rejeté"
                    : !v_StopIds.Contains(v_StopTime.StopId) ? $"trip_id={v_StopTime.TripId} : stop_id {v_StopTime.StopId} inconnu"
                    : !v_SeenStopTimes.Add((v_StopTime.TripId, v_StopTime.StopSequence)) ? $"trip_id={v_StopTime.TripId} : stop_sequence {v_StopTime.StopSequence} en double"
                    : null;
                if (v_Reason is null)
                {
                    v_StopTimes.Add(v_StopTime);
                    continue;
                }
                v_RejectedStopTimes++;
                v_Rejections.Add(new GtfsRejection("stop_times.txt", 0, v_Reason));
            }

            if (v_Routes.Count == 0 || v_Stops.Count == 0 || v_Trips.Count == 0 || v_StopTimes.Count == 0)
                throw new GtfsFormatException($"Flux incomplet après validation : {v_Routes.Count} lignes, {v_Stops.Count} arrêts, {v_Trips.Count} courses, {v_StopTimes.Count} horaires");

            double v_Ratio = (double)v_RejectedStopTimes / p_Feed.StopTimes.Count;
            if (v_Ratio > MaxRejectedStopTimeRatio)
                throw new GtfsFormatException($"{v_RejectedStopTimes} horaires rejetés sur {p_Feed.StopTimes.Count} ({v_Ratio:P1}), seuil {MaxRejectedStopTimeRatio:P0}");

            return new GtfsFeed
            {
                Routes = v_Routes,
                Stops = v_Stops,
                Trips = v_Trips,
                StopTimes = v_StopTimes,
                Calendars = v_Calendars,
                CalendarDates = p_Feed.CalendarDates,
                ShapePoints = p_Feed.ShapePoints,
                Rejections = v_Rejections
            };
        }

        private static List<T> Distinct<T>(IEnumerable<T> p_Items, Func<T, string> p_Key, string p_File, string p_KeyName, List<GtfsRejection> p_Rejections)
        {
            HashSet<string> v_Seen = [];
            List<T> v_Result = [];
            foreach (T v_Item in p_Items)
            {
                if (v_Seen.Add(p_Key(v_Item)))
                    v_Result.Add(v_Item);
                else
                    p_Rejections.Add(new GtfsRejection(p_File, 0, $"{p_KeyName} {p_Key(v_Item)} en double, seconde occurrence ignorée"));
            }
            return v_Result;
        }
    }
}
