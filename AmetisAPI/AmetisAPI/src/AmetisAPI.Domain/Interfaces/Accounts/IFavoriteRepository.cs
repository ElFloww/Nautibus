using AmetisAPI.Domain.Bo.Accounts;

namespace AmetisAPI.Domain.Interfaces.Accounts
{
    /// <summary>Favoris d'un utilisateur ; toujours filtrés par son identifiant (isolation des données personnelles).</summary>
    public interface IFavoriteRepository
    {
        Task<IReadOnlyList<Favorite>> ListAsync(string p_UserId, CancellationToken p_CancellationToken);

        /// <summary>Retourne false si le favori existait déjà.</summary>
        Task<bool> AddAsync(string p_UserId, FavoriteKind p_Kind, string p_TargetId, DateTime p_NowUtc, CancellationToken p_CancellationToken);

        /// <summary>Retourne false si le favori n'existait pas.</summary>
        Task<bool> RemoveAsync(string p_UserId, FavoriteKind p_Kind, string p_TargetId, CancellationToken p_CancellationToken);
    }
}
