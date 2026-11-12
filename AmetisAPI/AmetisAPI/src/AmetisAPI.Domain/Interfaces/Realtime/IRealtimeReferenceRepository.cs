using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Interfaces.Realtime
{
    /// <summary>Lien entre les identifiants du temps réel et ceux du réseau de référence.</summary>
    public interface IRealtimeReferenceRepository
    {
        Task<IReadOnlyList<RealtimeLineMatch>> GetLineMatchesAsync(CancellationToken p_CancellationToken);

        Task<IReadOnlyList<RealtimeTripInfo>> GetTripsAsync(IReadOnlyCollection<string> p_TripIds, CancellationToken p_CancellationToken);

        /// <summary>Nom et position des arrêts du flux temps réel actif.</summary>
        Task<IReadOnlyList<RealtimeStopInfo>> GetRealtimeStopsAsync(IReadOnlyCollection<string> p_StopIds, CancellationToken p_CancellationToken);

        /// <summary>Noms des arrêts du flux temps réel actif.</summary>
        Task<IReadOnlyDictionary<string, string>> GetRealtimeStopNamesAsync(IReadOnlyCollection<string> p_StopIds, CancellationToken p_CancellationToken);

        /// <summary>Arrêt du réseau rattaché à chaque arrêt du flux temps réel (absent si aucun rattachement).</summary>
        Task<IReadOnlyDictionary<string, string>> GetNetworkStopsForRealtimeStopsAsync(IReadOnlyCollection<string> p_RealtimeStopIds, CancellationToken p_CancellationToken);

        Task<IReadOnlyList<RealtimeStopMatch>> GetStopMatchesForNetworkStopsAsync(IReadOnlyCollection<string> p_NetworkStopIds, CancellationToken p_CancellationToken);
    }
}
