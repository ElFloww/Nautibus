using AmetisAPI.Domain.Bo.Accounts;

namespace AmetisAPI.Domain.Interfaces.Accounts
{
    public interface IAccountRepository
    {
        /// <summary>Null si le compte n'existe pas.</summary>
        Task<AccountInfo> GetAsync(string p_UserId, CancellationToken p_CancellationToken);

        /// <summary>Supprime le compte et ses données (favoris en cascade). False si le compte n'existe pas.</summary>
        Task<bool> DeleteAsync(string p_UserId, CancellationToken p_CancellationToken);
    }
}
