namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>Dernier état collecté des trois flux. Errors : flux indisponibles ou non décodables lors de cette collecte. RouteStates : suivi des bus par rapport à leur tracé (déviations).</summary>
    public record RealtimeSnapshot(
        DateTime? FeedTimestampUtc,
        DateTime CollectedAtUtc,
        IReadOnlyList<VehiclePosition> Vehicles,
        IReadOnlyList<StopTimePrediction> Predictions,
        IReadOnlyList<ServiceAlert> Alerts,
        IReadOnlyList<string> Anomalies,
        IReadOnlyList<string> Errors,
        IReadOnlyList<VehicleRouteState> RouteStates = null);
}
