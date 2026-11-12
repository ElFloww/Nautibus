using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class AccountRepository(UserManager<AppUser> p_Users) : IAccountRepository
    {
        public async Task<AccountInfo> GetAsync(string p_UserId, CancellationToken p_CancellationToken)
        {
            AppUser v_User = await p_Users.FindByIdAsync(p_UserId);
            return v_User is null ? null : new AccountInfo(v_User.Email, v_User.EmailConfirmed);
        }

        public async Task<bool> DeleteAsync(string p_UserId, CancellationToken p_CancellationToken)
        {
            AppUser v_User = await p_Users.FindByIdAsync(p_UserId);
            if (v_User is null)
                return false;
            IdentityResult v_Result = await p_Users.DeleteAsync(v_User);
            return v_Result.Succeeded ? true : throw new InvalidOperationException("Suppression du compte refusée : " + string.Join(", ", v_Result.Errors.Select(p_E => p_E.Code)));
        }
    }
}
