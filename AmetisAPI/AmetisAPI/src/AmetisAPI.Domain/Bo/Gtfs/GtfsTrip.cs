namespace AmetisAPI.Domain.Bo.Gtfs
{
    public record GtfsTrip(string TripId, string RouteId, string ServiceId, string Headsign, int? DirectionId, string ShapeId);
}
