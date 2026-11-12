namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>Arrêt d'une course : heure prévue, heure effective publiée (réelle si desservi, estimée sinon) et écart. Position de l'arrêt pour l'affichage sur la carte.</summary>
    public record TripStopView(int Order, string StopName, string NetworkStopId, DateTimeOffset ScheduledTime, DateTimeOffset ExpectedTime, int DelaySeconds, TripStopStatus Status, double? Latitude, double? Longitude);
}
