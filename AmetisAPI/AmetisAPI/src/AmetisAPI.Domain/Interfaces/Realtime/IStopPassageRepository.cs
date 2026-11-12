using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Interfaces.Realtime
{
    /// <summary>Historique des passages (conservé 30 jours), mis à jour à chaque collecte.</summary>
    public interface IStopPassageRepository
    {
        /// <summary>Crée ou met à jour chaque passage (clé : course, arrêt, heure prévue).</summary>
        Task<int> UpsertAsync(IReadOnlyCollection<StopPassage> p_Passages, DateTime p_NowUtc, CancellationToken p_CancellationToken);

        /// <summary>Passages de la dernière circulation de la course mise à jour, dans l'ordre des arrêts.</summary>
        Task<IReadOnlyList<StopPassage>> ListForTripAsync(string p_TripId, DateTime p_NowUtc, CancellationToken p_CancellationToken);

        /// <summary>Passages aux arrêts donnés dont l'heure effective est dans l'intervalle.</summary>
        Task<IReadOnlyList<StopPassage>> ListForStopsAsync(IReadOnlyCollection<string> p_StopIds, DateTime p_FromUtc, DateTime p_ToUtc, CancellationToken p_CancellationToken);

        Task<int> PurgeOlderThanAsync(DateTime p_LimitUtc, CancellationToken p_CancellationToken);
    }
}
