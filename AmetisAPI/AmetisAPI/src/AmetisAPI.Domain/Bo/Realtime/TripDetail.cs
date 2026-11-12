using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>
    /// Course suivie en temps réel : arrêts desservis et à venir, position actuelle du bus (si visible) et
    /// trace de ses positions enregistrées ([longitude, latitude]).
    /// </summary>
    public record TripDetail(
        string TripId,
        LineSummary Line,
        MatchConfidence LineConfidence,
        string Destination,
        int? DirectionId,
        string VehicleId,
        double? Latitude,
        double? Longitude,
        double? Bearing,
        int? PositionAgeSeconds,
        int? FeedAgeSeconds,
        bool IsStale,
        IReadOnlyList<TripStopView> Stops,
        IReadOnlyList<double[]> Trace);
}
