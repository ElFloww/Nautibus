using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>Prochain passage temps réel, avec sa provenance et son niveau de confiance.</summary>
    public record RealtimeArrival(
        string TripId,
        string StopId,
        LineSummary Line,
        string Destination,
        DateTimeOffset ExpectedTime,
        int? DelaySeconds,
        string Provenance,
        MatchConfidence Confidence,
        int FeedAgeSeconds,
        bool IsStale);
}
