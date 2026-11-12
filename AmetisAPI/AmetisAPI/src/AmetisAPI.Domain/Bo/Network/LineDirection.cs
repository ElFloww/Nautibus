namespace AmetisAPI.Domain.Bo.Network
{
    /// <summary>Parcours type d'une direction : la course desservant le plus d'arrêts sur le tracé le plus fréquent.</summary>
    public record LineDirection(int DirectionId, string Destination, IReadOnlyList<StopSummary> Stops);
}
