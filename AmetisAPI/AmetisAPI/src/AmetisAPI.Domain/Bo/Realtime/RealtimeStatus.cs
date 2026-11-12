namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>État du temps réel exposé à l'interface : fraîcheur et disponibilité.</summary>
    public record RealtimeStatus(DateTime? FeedTimestampUtc, DateTime? CollectedAtUtc, int? FeedAgeSeconds, bool IsStale, int VehicleCount, int AlertCount, IReadOnlyList<string> Errors);
}
