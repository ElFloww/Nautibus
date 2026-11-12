namespace AmetisAPI.Domain.Bo.Realtime
{
    public record RealtimeStopMatch(string RealtimeStopId, string NetworkStopId, double DistanceMeters, MatchConfidence Confidence);
}
