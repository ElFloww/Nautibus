using System.Text.Json;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Realtime;
using StackExchange.Redis;

namespace AmetisAPI.Infrastructure.Services
{
    /// <summary>Dernier état temps réel partagé entre le Worker (écriture) et l'API (lecture).</summary>
    internal class RedisRealtimeStore(IConnectionMultiplexer p_Redis) : IRealtimeStore
    {
        private const string Key = "ametis:realtime:snapshot";

        /// <summary>Sans collecte pendant ce délai, la clé disparaît : l'interface affiche « pas de temps réel » plutôt que des données anciennes.</summary>
        private static readonly TimeSpan m_Expiry = TimeSpan.FromHours(1);

        public Task SaveAsync(RealtimeSnapshot p_Snapshot, CancellationToken p_CancellationToken)
        {
            return p_Redis.GetDatabase().StringSetAsync(Key, JsonSerializer.Serialize(p_Snapshot), m_Expiry);
        }

        public async Task<RealtimeSnapshot> GetAsync(CancellationToken p_CancellationToken)
        {
            RedisValue v_Value = await p_Redis.GetDatabase().StringGetAsync(Key);
            return v_Value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<RealtimeSnapshot>(v_Value.ToString());
        }
    }
}
