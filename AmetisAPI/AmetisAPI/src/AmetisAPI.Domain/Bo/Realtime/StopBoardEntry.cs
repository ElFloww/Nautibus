using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>
    /// Passage à un arrêt : heure prévue toujours présente ; heure effective et écart quand Ametis publie la course en temps réel
    /// (IsRealtime), sinon horaire théorique seul. TripId : course du flux temps réel (détail de la course).
    /// </summary>
    public record StopBoardEntry(
        LineSummary Line,
        string Destination,
        DateTimeOffset ScheduledTime,
        DateTimeOffset? ExpectedTime,
        int? DelaySeconds,
        string TripId,
        bool IsRealtime,
        MatchConfidence Confidence,
        bool IsStale);
}
