using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Domain.Interfaces.Gtfs
{
    /// <summary>Stockage versionné des flux GTFS : une version n'est visible qu'une fois activée.</summary>
    public interface IFeedRepository
    {
        Task<FeedVersionInfo> GetAsync(int p_VersionId, CancellationToken p_CancellationToken);

        Task<FeedVersionInfo> GetActiveAsync(FeedSource p_Source, CancellationToken p_CancellationToken);

        Task<int> StartVersionAsync(FeedSource p_Source, string p_SourceUrl, string p_ContentHash, CancellationToken p_CancellationToken);

        Task MarkSkippedAsync(int p_VersionId, CancellationToken p_CancellationToken);

        Task WriteContentAsync(int p_VersionId, GtfsFeed p_Feed, CancellationToken p_CancellationToken);

        /// <summary>Active la version, désactive l'ancienne et purge ses lignes (le journal est conservé).</summary>
        Task ActivateAsync(int p_VersionId, FeedSource p_Source, string p_ChangeSummary, CancellationToken p_CancellationToken);

        /// <summary>Marque l'échec et supprime les lignes partielles de cette version uniquement.</summary>
        Task FailAsync(int p_VersionId, string p_ErrorMessage, CancellationToken p_CancellationToken);

        /// <summary>Recalcule la correspondance arrêts et lignes entre les deux flux actifs.</summary>
        Task<int> RebuildMatchesAsync(CancellationToken p_CancellationToken);

        Task<IReadOnlyList<string>> GetRouteIdsAsync(int p_VersionId, CancellationToken p_CancellationToken);

        Task<IReadOnlyList<string>> GetStopIdsAsync(int p_VersionId, CancellationToken p_CancellationToken);

        Task<IReadOnlyList<FeedVersionInfo>> ListRecentAsync(int p_Count, CancellationToken p_CancellationToken);
    }
}
