namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>Position telle que publiée par le flux GTFS-RT (identifiants du GTFS « pour le GTFS-RT »).</summary>
    public record VehiclePosition(string VehicleId, string Label, string TripId, string RouteId, double Latitude, double Longitude, double? Bearing, double? SpeedMetersPerSecond, string StopId, DateTime TimestampUtc);
}
