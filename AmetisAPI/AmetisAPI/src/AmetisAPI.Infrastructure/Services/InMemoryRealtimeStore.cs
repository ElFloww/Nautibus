using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Infrastructure.Services
{
    /// <summary>
    /// shortcut: état en mémoire du processus, valable seulement si la collecte tourne dans ce même processus
    /// (développement sans Redis) ; configurer ConnectionStrings:Redis dès que l'API et le Worker sont séparés.
    /// </summary>
    internal class InMemoryRealtimeStore : IRealtimeStore
    {
        private volatile RealtimeSnapshot m_Snapshot;

        public Task SaveAsync(RealtimeSnapshot p_Snapshot, CancellationToken p_CancellationToken)
        {
            m_Snapshot = p_Snapshot;
            return Task.CompletedTask;
        }

        public Task<RealtimeSnapshot> GetAsync(CancellationToken p_CancellationToken) => Task.FromResult(m_Snapshot);
    }
}
