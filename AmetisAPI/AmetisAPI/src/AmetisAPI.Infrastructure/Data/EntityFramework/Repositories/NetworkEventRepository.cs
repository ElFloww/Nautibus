using System.Text.Json;
using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Events;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class NetworkEventRepository(AppDbContext p_Context) : INetworkEventRepository
    {
        public async Task<IReadOnlyList<NetworkEvent>> ListAsync(DateTime p_SinceUtc, CancellationToken p_CancellationToken)
        {
            List<NetworkEventEntity> v_Entities = await p_Context.NetworkEvents.AsNoTracking()
                .Where(p_E => p_E.ClosedUtc == null || p_E.ClosedUtc >= p_SinceUtc)
                .ToListAsync(p_CancellationToken);
            return v_Entities.Select(ToBo).ToList();
        }

        public async Task<IReadOnlyList<NetworkEvent>> SyncAsync(NetworkEventKind p_Kind, IReadOnlyList<NetworkEventDraft> p_Current, DateTime p_NowUtc, TimeSpan p_CloseAfter, CancellationToken p_CancellationToken)
        {
            Dictionary<string, NetworkEventEntity> v_Open = await p_Context.NetworkEvents
                .Where(p_E => p_E.Kind == (byte)p_Kind && p_E.ClosedUtc == null)
                .ToDictionaryAsync(p_E => p_E.ExternalId, p_CancellationToken);

            List<NetworkEventEntity> v_Created = [];
            foreach (NetworkEventDraft v_Draft in p_Current.DistinctBy(p_D => p_D.ExternalId))
            {
                if (!v_Open.Remove(v_Draft.ExternalId, out NetworkEventEntity v_Entity))
                {
                    v_Entity = new NetworkEventEntity
                    {
                        Kind = (byte)p_Kind,
                        ExternalId = v_Draft.ExternalId,
                        StartUtc = v_Draft.StartUtc ?? p_NowUtc,
                        CreatedUtc = p_NowUtc
                    };
                    p_Context.NetworkEvents.Add(v_Entity);
                    v_Created.Add(v_Entity);
                }
                v_Entity.Title = v_Draft.Title;
                v_Entity.Description = v_Draft.Description;
                v_Entity.Url = v_Draft.Url;
                v_Entity.LineIds = JsonSerializer.Serialize(v_Draft.LineIds);
                v_Entity.EndUtc = v_Draft.EndUtc;
                v_Entity.Confidence = (byte)v_Draft.Confidence;
                v_Entity.LastSeenUtc = p_NowUtc;
            }

            // Évènements qui ne sont plus constatés : clos après le délai de grâce.
            foreach (NetworkEventEntity v_Gone in v_Open.Values.Where(p_E => p_NowUtc - p_E.LastSeenUtc >= p_CloseAfter))
                v_Gone.ClosedUtc = p_NowUtc;

            await p_Context.SaveChangesAsync(p_CancellationToken);
            p_Context.ChangeTracker.Clear();
            return v_Created.Select(ToBo).ToList();
        }

        public Task<int> PurgeClosedBeforeAsync(DateTime p_LimitUtc, CancellationToken p_CancellationToken)
        {
            return p_Context.NetworkEvents.Where(p_E => p_E.ClosedUtc < p_LimitUtc).ExecuteDeleteAsync(p_CancellationToken);
        }

        private static NetworkEvent ToBo(NetworkEventEntity p_E) => new(
            p_E.Id, (NetworkEventKind)p_E.Kind, p_E.ExternalId, p_E.Title, p_E.Description, p_E.Url,
            JsonSerializer.Deserialize<List<string>>(p_E.LineIds ?? "[]"), p_E.StartUtc, p_E.EndUtc, p_E.ClosedUtc, p_E.LastSeenUtc, (MatchConfidence)p_E.Confidence);
    }
}
