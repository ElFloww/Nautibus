using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Domain.Bo.Realtime
{
    public record VehicleView(
        string VehicleId,
        string Label,
        LineSummary Line,
        string Destination,
        int? DirectionId,
        double Latitude,
        double Longitude,
        double? Bearing,
        double? SpeedKmh,
        DateTime TimestampUtc,
        int AgeSeconds,
        bool IsStale,
        MatchConfidence LineConfidence,
        int? DelaySeconds,
        string NextStopName,
        bool IsOffRoute,
        double? OffRouteMeters,
        IReadOnlyList<double[]> OffRouteTrack,
        string TripId);
}
