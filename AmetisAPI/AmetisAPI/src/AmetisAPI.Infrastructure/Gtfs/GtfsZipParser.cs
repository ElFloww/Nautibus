using System.IO.Compression;
using System.Text;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Exceptions;
using AmetisAPI.Domain.Interfaces.Gtfs;

namespace AmetisAPI.Infrastructure.Gtfs
{
    /// <summary>
    /// Lecture d'une archive GTFS. Fichiers ou colonnes obligatoires absents : <see cref="GtfsFormatException"/>.
    /// Valeur illisible : ligne rejetée et consignée. Seuls les champs utilisés par l'application sont lus.
    /// </summary>
    internal class GtfsZipParser : IGtfsParser
    {
        public GtfsFeed Parse(Stream p_ZipStream)
        {
            ZipArchive v_Zip;
            try
            {
                v_Zip = new ZipArchive(p_ZipStream, ZipArchiveMode.Read);
            }
            catch (InvalidDataException v_Ex)
            {
                throw new GtfsFormatException($"Archive ZIP illisible : {v_Ex.Message}");
            }

            using (v_Zip)
            {
                List<GtfsRejection> v_Rejections = [];
                Open(v_Zip, "agency.txt", true, "agency_name");

                GtfsFeed v_Feed = new()
                {
                    Routes = Open(v_Zip, "routes.txt", true, "route_id", "route_type").Map(ReadRoute, v_Rejections),
                    Stops = Open(v_Zip, "stops.txt", true, "stop_id", "stop_lat", "stop_lon").Map(ReadStop, v_Rejections),
                    Trips = Open(v_Zip, "trips.txt", true, "route_id", "service_id", "trip_id").Map(ReadTrip, v_Rejections),
                    StopTimes = Open(v_Zip, "stop_times.txt", true, "trip_id", "stop_sequence").Map(ReadStopTime, v_Rejections),
                    Calendars = Open(v_Zip, "calendar.txt", false, "service_id", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday", "start_date", "end_date")?.Map(ReadCalendar, v_Rejections) ?? [],
                    CalendarDates = Open(v_Zip, "calendar_dates.txt", false, "service_id", "date", "exception_type")?.Map(ReadCalendarDate, v_Rejections) ?? [],
                    ShapePoints = Open(v_Zip, "shapes.txt", false, "shape_id", "shape_pt_lat", "shape_pt_lon", "shape_pt_sequence")?.Map(ReadShapePoint, v_Rejections) ?? [],
                    Rejections = v_Rejections
                };

                if (v_Zip.GetEntry("calendar.txt") is null && v_Zip.GetEntry("calendar_dates.txt") is null)
                    throw new GtfsFormatException("calendar.txt et calendar_dates.txt absents : aucun calendrier de service");
                return v_Feed;
            }
        }

        private static GtfsTable Open(ZipArchive p_Zip, string p_File, bool p_Required, params string[] p_Columns)
        {
            ZipArchiveEntry v_Entry = p_Zip.GetEntry(p_File);
            if (v_Entry is null)
                return p_Required ? throw new GtfsFormatException($"Fichier obligatoire {p_File} absent") : null;
            using StreamReader v_Reader = new(v_Entry.Open(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return new GtfsTable(p_File, v_Reader, p_Columns);
        }

        private static GtfsRoute ReadRoute(GtfsTable.Row p_Row)
        {
            string v_Short = p_Row.Get("route_short_name");
            string v_Long = p_Row.Get("route_long_name");
            if (v_Short is null && v_Long is null)
                throw new FormatException("route_short_name et route_long_name vides");
            return new GtfsRoute(p_Row.Required("route_id"), v_Short, v_Long, p_Row.Int("route_type") ?? throw new FormatException("route_type vide"),
                p_Row.Get("route_color"), p_Row.Get("route_text_color"), p_Row.Int("route_sort_order"));
        }

        private static GtfsStop ReadStop(GtfsTable.Row p_Row)
        {
            int v_LocationType = p_Row.Int("location_type") ?? 0;
            if (v_LocationType > 2)
                throw new FormatException($"location_type {v_LocationType} (nœud ou zone d'embarquement) non pris en charge");
            double v_Lat = p_Row.Double("stop_lat") ?? throw new FormatException("stop_lat vide");
            double v_Lon = p_Row.Double("stop_lon") ?? throw new FormatException("stop_lon vide");
            if (v_Lat is < -90 or > 90 || v_Lon is < -180 or > 180)
                throw new FormatException(FormattableString.Invariant($"coordonnées hors limites ({v_Lat}, {v_Lon})"));
            return new GtfsStop(p_Row.Required("stop_id"), p_Row.Get("stop_code"), p_Row.Get("stop_name") ?? throw new FormatException("stop_name vide"),
                v_Lat, v_Lon, v_LocationType, p_Row.Get("parent_station"), p_Row.Int("wheelchair_boarding") ?? 0);
        }

        private static GtfsTrip ReadTrip(GtfsTable.Row p_Row)
        {
            return new GtfsTrip(p_Row.Required("trip_id"), p_Row.Required("route_id"), p_Row.Required("service_id"),
                p_Row.Get("trip_headsign"), p_Row.Int("direction_id"), p_Row.Get("shape_id"));
        }

        private static GtfsStopTime ReadStopTime(GtfsTable.Row p_Row)
        {
            string v_StopId = p_Row.Get("stop_id")
                ?? throw new FormatException(p_Row.Get("location_id") is null && p_Row.Get("location_group_id") is null
                    ? "stop_id vide"
                    : "desserte de zone flexible (location_id) non prise en charge");
            return new GtfsStopTime(p_Row.Required("trip_id"), p_Row.Int("stop_sequence") ?? throw new FormatException("stop_sequence vide"), v_StopId,
                p_Row.Time("arrival_time"), p_Row.Time("departure_time"), p_Row.Int("pickup_type") ?? 0, p_Row.Int("drop_off_type") ?? 0,
                p_Row.Double("shape_dist_traveled"));
        }

        private static readonly string[] m_Days = ["monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday"];

        private static GtfsCalendar ReadCalendar(GtfsTable.Row p_Row)
        {
            int v_Mask = 0;
            for (int v_I = 0; v_I < m_Days.Length; v_I++)
            {
                int v_Day = p_Row.Int(m_Days[v_I]) ?? throw new FormatException($"{m_Days[v_I]} vide");
                if (v_Day is not (0 or 1))
                    throw new FormatException($"{m_Days[v_I]} doit valoir 0 ou 1");
                v_Mask |= v_Day << v_I;
            }
            return new GtfsCalendar(p_Row.Required("service_id"), v_Mask, p_Row.Date("start_date"), p_Row.Date("end_date"));
        }

        private static GtfsCalendarDate ReadCalendarDate(GtfsTable.Row p_Row)
        {
            int v_Type = p_Row.Int("exception_type") ?? throw new FormatException("exception_type vide");
            if (v_Type is not (1 or 2))
                throw new FormatException($"exception_type {v_Type} invalide");
            return new GtfsCalendarDate(p_Row.Required("service_id"), p_Row.Date("date"), v_Type);
        }

        private static GtfsShapePoint ReadShapePoint(GtfsTable.Row p_Row)
        {
            return new GtfsShapePoint(p_Row.Required("shape_id"), p_Row.Int("shape_pt_sequence") ?? throw new FormatException("shape_pt_sequence vide"),
                p_Row.Double("shape_pt_lat") ?? throw new FormatException("shape_pt_lat vide"),
                p_Row.Double("shape_pt_lon") ?? throw new FormatException("shape_pt_lon vide"));
        }
    }
}
