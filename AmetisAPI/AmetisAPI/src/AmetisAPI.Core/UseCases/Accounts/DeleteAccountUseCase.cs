using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases.Accounts
{
    /// <summary>Droit à l'effacement : suppression du compte et de ses favoris. Aucune donnée personnelle n'est journalisée.</summary>
    internal class DeleteAccountUseCase(ILogger<DeleteAccountUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IAccountRepository p_Accounts)
        : ADbUseCase<DeleteAccountRequest, ValueResponse<bool>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<bool>> HandleLogic(DeleteAccountRequest p_Request, CancellationToken p_CancellationToken)
        {
            bool v_Deleted = await WithDatabaseAsync(() => p_Accounts.DeleteAsync(p_Request.UserId, p_CancellationToken), p_CancellationToken);
            if (!v_Deleted)
                return ValueResponse<bool>.Error(DomainErrors.Data.NotFound, "Compte inconnu");
            m_Logger.LogInformation("Compte utilisateur supprimé à la demande de son titulaire");
            return new(true);
        }
    }
}
