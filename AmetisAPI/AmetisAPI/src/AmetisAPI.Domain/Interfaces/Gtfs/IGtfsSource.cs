using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Domain.Interfaces.Gtfs
{
    /// <summary>Téléchargement du fichier GTFS d'une source.</summary>
    public interface IGtfsSource
    {
        Task<byte[]> DownloadAsync(FeedSource p_Source, CancellationToken p_CancellationToken);

        string GetUrl(FeedSource p_Source);
    }
}
