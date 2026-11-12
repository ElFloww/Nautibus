namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>
    /// Suivi d'un véhicule par rapport au tracé de sa ligne : nombre de positions successives hors tracé,
    /// distance au tracé et points hors tracé ([longitude, latitude]) pour dessiner l'itinéraire de déviation.
    /// </summary>
    public record VehicleRouteState(string VehicleId, string LineId, int OffRouteCount, DateTime LastTimestampUtc, double DistanceMeters, IReadOnlyList<double[]> Track);
}
