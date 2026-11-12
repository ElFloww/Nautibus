namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>Tableau d'un arrêt : prochains passages et passages des 30 dernières minutes.</summary>
    public record StopBoard(IReadOnlyList<StopBoardEntry> Upcoming, IReadOnlyList<StopBoardEntry> Recent, int? FeedAgeSeconds);
}
