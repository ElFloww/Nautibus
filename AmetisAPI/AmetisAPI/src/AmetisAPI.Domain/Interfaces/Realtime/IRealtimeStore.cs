using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Interfaces.Realtime
{
    /// <summary>Dernier état temps réel (Redis en production).</summary>
    public interface IRealtimeStore
    {
        Task SaveAsync(RealtimeSnapshot p_Snapshot, CancellationToken p_CancellationToken);

        /// <summary>Null si aucune collecte n'a encore eu lieu.</summary>
        Task<RealtimeSnapshot> GetAsync(CancellationToken p_CancellationToken);
    }
}
