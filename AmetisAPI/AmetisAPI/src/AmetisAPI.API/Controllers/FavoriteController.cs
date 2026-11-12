using System.Security.Claims;
using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Bo.Accounts;
using Microsoft.AspNetCore.Authorization;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Favoris de l'utilisateur connecté (arrêts et lignes).</summary>
    [ApiVersion("1.0")]
    [Authorize]
    public class FavoriteController(ILogger<FavoriteController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>Liste des favoris.</summary>
        [HttpGet]
        public Task<IActionResult> ListAsync(CancellationToken p_CancellationToken)
            => SendAsync<ListFavoritesRequest, IReadOnlyList<Favorite>>(new ListFavoritesRequest(UserId), p_CancellationToken);

        /// <summary>Ajoute un favori ; renvoie false s'il existait déjà.</summary>
        [HttpPut("{kind}/{targetId}")]
        public Task<IActionResult> AddAsync([FromRoute(Name = "kind")] FavoriteKind p_Kind, [FromRoute(Name = "targetId")] string p_TargetId, CancellationToken p_CancellationToken)
            => SendAsync<AddFavoriteRequest, bool>(new AddFavoriteRequest(UserId, p_Kind, p_TargetId), p_CancellationToken);

        /// <summary>Retire un favori.</summary>
        [HttpDelete("{kind}/{targetId}")]
        public Task<IActionResult> RemoveAsync([FromRoute(Name = "kind")] FavoriteKind p_Kind, [FromRoute(Name = "targetId")] string p_TargetId, CancellationToken p_CancellationToken)
            => SendAsync<RemoveFavoriteRequest, bool>(new RemoveFavoriteRequest(UserId, p_Kind, p_TargetId), p_CancellationToken);
    }
}
