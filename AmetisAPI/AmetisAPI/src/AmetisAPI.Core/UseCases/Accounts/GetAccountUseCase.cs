using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases.Accounts
{
    internal class GetAccountUseCase(ILogger<GetAccountUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IAccountRepository p_Accounts)
        : ADbUseCase<GetAccountRequest, ValueResponse<AccountInfo>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<AccountInfo>> HandleLogic(GetAccountRequest p_Request, CancellationToken p_CancellationToken)
        {
            AccountInfo v_Account = await WithDatabaseAsync(() => p_Accounts.GetAsync(p_Request.UserId, p_CancellationToken), p_CancellationToken);
            return v_Account is null ? ValueResponse<AccountInfo>.Error(DomainErrors.Data.Unauthorized, "Compte inconnu") : new(v_Account);
        }
    }
}
