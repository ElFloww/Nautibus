namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>État des données pour la supervision : imports en échec, données expirées, temps réel obsolète.</summary>
    public record DataHealth(DataHealthStatus Status, IReadOnlyList<string> Messages);
}
