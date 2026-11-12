using AmetisAPI.Domain.Bo.Events;

namespace AmetisAPI.Domain.Interfaces.Events
{
    public interface INetworkEventRepository
    {
        /// <summary>Évènements encore ouverts ou terminés depuis <paramref name="p_SinceUtc"/>.</summary>
        Task<IReadOnlyList<NetworkEvent>> ListAsync(DateTime p_SinceUtc, CancellationToken p_CancellationToken);

        /// <summary>
        /// Rapproche les évènements constatés de ceux ouverts du même type (par ExternalId) : création, mise à jour de
        /// LastSeenUtc, fermeture de ceux absents depuis plus de <paramref name="p_CloseAfter"/>. Retourne les évènements créés.
        /// </summary>
        Task<IReadOnlyList<NetworkEvent>> SyncAsync(NetworkEventKind p_Kind, IReadOnlyList<NetworkEventDraft> p_Current, DateTime p_NowUtc, TimeSpan p_CloseAfter, CancellationToken p_CancellationToken);

        Task<int> PurgeClosedBeforeAsync(DateTime p_LimitUtc, CancellationToken p_CancellationToken);
    }
}
