namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>Entrée du journal d'import.</summary>
    public record FeedVersionInfo(
        int Id,
        FeedSource Source,
        FeedImportStatus Status,
        bool IsActive,
        string ContentHash,
        DateTime StartedAtUtc,
        DateTime? FinishedAtUtc,
        int RouteCount,
        int StopCount,
        int TripCount,
        int StopTimeCount,
        int RejectedCount,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        string ChangeSummary,
        string ErrorMessage);
}
