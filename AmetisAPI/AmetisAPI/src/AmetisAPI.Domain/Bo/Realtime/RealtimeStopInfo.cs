namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>Arrêt du flux temps réel : nom et position.</summary>
    public record RealtimeStopInfo(string StopId, string Name, double Latitude, double Longitude);
}
