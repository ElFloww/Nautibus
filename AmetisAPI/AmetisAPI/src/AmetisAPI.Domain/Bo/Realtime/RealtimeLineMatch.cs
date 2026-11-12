using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Domain.Bo.Realtime
{
    public record RealtimeLineMatch(string RealtimeRouteId, LineSummary Line, MatchConfidence Confidence);
}
