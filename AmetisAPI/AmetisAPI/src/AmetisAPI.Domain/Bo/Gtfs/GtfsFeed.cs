namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>Contenu lu d'un fichier GTFS, avant validation métier.</summary>
    public class GtfsFeed
    {
        public List<GtfsRoute> Routes { get; init; } = [];
        public List<GtfsStop> Stops { get; init; } = [];
        public List<GtfsTrip> Trips { get; init; } = [];
        public List<GtfsStopTime> StopTimes { get; init; } = [];
        public List<GtfsCalendar> Calendars { get; init; } = [];
        public List<GtfsCalendarDate> CalendarDates { get; init; } = [];
        public List<GtfsShapePoint> ShapePoints { get; init; } = [];
        public List<GtfsRejection> Rejections { get; init; } = [];
    }
}
