using System.Security.Cryptography;
using System.Text;
using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Bo.Push;
using AmetisAPI.Domain.Interfaces.Push;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class PushSubscriptionRepository(AppDbContext p_Context) : IPushSubscriptionRepository
    {
        public async Task SaveAsync(string p_UserId, PushSubscriptionInfo p_Subscription, bool p_AllLines, DateTime p_NowUtc, CancellationToken p_CancellationToken)
        {
            string v_Hash = Hash(p_Subscription.Endpoint);
            PushSubscriptionEntity v_Entity = await p_Context.PushSubscriptions.SingleOrDefaultAsync(p_S => p_S.EndpointHash == v_Hash, p_CancellationToken);
            if (v_Entity is null)
            {
                v_Entity = new PushSubscriptionEntity { Endpoint = p_Subscription.Endpoint, EndpointHash = v_Hash, CreatedAtUtc = p_NowUtc };
                p_Context.PushSubscriptions.Add(v_Entity);
            }
            // Un navigateur n'a qu'un abonnement : il suit le compte connecté en dernier.
            v_Entity.UserId = p_UserId;
            v_Entity.P256dh = p_Subscription.P256dh;
            v_Entity.Auth = p_Subscription.Auth;
            v_Entity.AllLines = p_AllLines;
            await p_Context.SaveChangesAsync(p_CancellationToken);
            p_Context.ChangeTracker.Clear();
        }

        public async Task<bool> RemoveAsync(string p_UserId, string p_Endpoint, CancellationToken p_CancellationToken)
        {
            string v_Hash = Hash(p_Endpoint);
            return await p_Context.PushSubscriptions.Where(p_S => p_S.EndpointHash == v_Hash && p_S.UserId == p_UserId).ExecuteDeleteAsync(p_CancellationToken) > 0;
        }

        public async Task<IReadOnlyList<PushSubscriptionInfo>> ListForLinesAsync(IReadOnlyCollection<string> p_LineIds, CancellationToken p_CancellationToken)
        {
            List<string> v_Lines = [.. p_LineIds];
            return await p_Context.PushSubscriptions
                .Where(p_S => p_S.AllLines || p_Context.Favorites.Any(p_F => p_F.UserId == p_S.UserId && p_F.Kind == (byte)FavoriteKind.Line && v_Lines.Contains(p_F.TargetId)))
                .Select(p_S => new PushSubscriptionInfo(p_S.Endpoint, p_S.P256dh, p_S.Auth))
                .ToListAsync(p_CancellationToken);
        }

        public Task RemoveByEndpointAsync(string p_Endpoint, CancellationToken p_CancellationToken)
        {
            string v_Hash = Hash(p_Endpoint);
            return p_Context.PushSubscriptions.Where(p_S => p_S.EndpointHash == v_Hash).ExecuteDeleteAsync(p_CancellationToken);
        }

        private static string Hash(string p_Endpoint) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(p_Endpoint)));
    }
}
