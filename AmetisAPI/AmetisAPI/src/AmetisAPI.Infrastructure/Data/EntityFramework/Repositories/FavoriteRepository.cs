using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class FavoriteRepository(AppDbContext p_Context) : IFavoriteRepository
    {
        public async Task<IReadOnlyList<Favorite>> ListAsync(string p_UserId, CancellationToken p_CancellationToken)
        {
            return await p_Context.Favorites.Where(p_F => p_F.UserId == p_UserId).OrderBy(p_F => p_F.CreatedAtUtc)
                .Select(p_F => new Favorite((FavoriteKind)p_F.Kind, p_F.TargetId, p_F.CreatedAtUtc))
                .ToListAsync(p_CancellationToken);
        }

        public async Task<bool> AddAsync(string p_UserId, FavoriteKind p_Kind, string p_TargetId, DateTime p_NowUtc, CancellationToken p_CancellationToken)
        {
            if (await p_Context.Favorites.AnyAsync(p_F => p_F.UserId == p_UserId && p_F.Kind == (byte)p_Kind && p_F.TargetId == p_TargetId, p_CancellationToken))
                return false;
            p_Context.Favorites.Add(new FavoriteEntity { UserId = p_UserId, Kind = (byte)p_Kind, TargetId = p_TargetId, CreatedAtUtc = p_NowUtc });
            await p_Context.SaveChangesAsync(p_CancellationToken);
            p_Context.ChangeTracker.Clear();
            return true;
        }

        public async Task<bool> RemoveAsync(string p_UserId, FavoriteKind p_Kind, string p_TargetId, CancellationToken p_CancellationToken)
        {
            return await p_Context.Favorites.Where(p_F => p_F.UserId == p_UserId && p_F.Kind == (byte)p_Kind && p_F.TargetId == p_TargetId)
                .ExecuteDeleteAsync(p_CancellationToken) > 0;
        }
    }
}
