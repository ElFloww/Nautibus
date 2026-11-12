using System.Security.Claims;
using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Bo.Accounts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Compte de l'utilisateur connecté. Inscription et connexion : endpoints Identity sous /api/v1/auth.</summary>
    [ApiVersion("1.0")]
    [Authorize]
    public class AccountController(ILogger<AccountController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>Informations du compte connecté.</summary>
        [HttpGet("me")]
        public Task<IActionResult> MeAsync(CancellationToken p_CancellationToken)
            => SendAsync<GetAccountRequest, AccountInfo>(new GetAccountRequest(UserId), p_CancellationToken);

        /// <summary>Supprime le compte et ses favoris, puis déconnecte.</summary>
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteAsync(CancellationToken p_CancellationToken)
        {
            IActionResult v_Result = await SendAsync<DeleteAccountRequest, bool>(new DeleteAccountRequest(UserId), p_CancellationToken);
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return v_Result;
        }

        /// <summary>Déconnexion (suppression du cookie).</summary>
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return NoContent();
        }
    }
}
