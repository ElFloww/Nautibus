namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>Contenu décodé d'un flux GTFS-RT. Anomalies : incohérences constatées, consignées sans correction.</summary>
    public record RealtimeFeed(DateTime? TimestampUtc, IReadOnlyList<VehiclePosition> Vehicles, IReadOnlyList<StopTimePrediction> Predictions, IReadOnlyList<ServiceAlert> Alerts, IReadOnlyList<string> Anomalies);
}
