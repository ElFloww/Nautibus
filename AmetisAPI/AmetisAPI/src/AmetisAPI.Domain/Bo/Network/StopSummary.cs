namespace AmetisAPI.Domain.Bo.Network
{
    /// <summary>Arrêt présenté à l'usager : zone d'arrêt (parent) ou quai isolé.</summary>
    public record StopSummary(string Id, string Name, double Latitude, double Longitude, IReadOnlyList<LineSummary> Lines);
}
