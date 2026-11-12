using System.Security.Claims;
using AmetisAPI.Core.Requests.Push;
using Microsoft.AspNetCore.Authorization;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Abonnement aux notifications push des infos trafic.</summary>
    [ApiVersion("1.0")]
    public class NotificationController(ILogger<NotificationController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>Clé publique VAPID à transmettre au navigateur (503 si le push n'est pas configuré).</summary>
        [HttpGet("key")]
        public Task<IActionResult> KeyAsync(CancellationToken p_CancellationToken)
            => SendAsync<GetPushKeyRequest, string>(new GetPushKeyRequest(), p_CancellationToken);

        /// <summary>Enregistre l'abonnement de ce navigateur pour l'utilisateur connecté.</summary>
        [Authorize]
        [HttpPut("subscription")]
        public Task<IActionResult> SubscribeAsync([FromBody] PushSubscriptionBody p_Body, CancellationToken p_CancellationToken)
            => SendAsync<SavePushSubscriptionRequest, bool>(new SavePushSubscriptionRequest(UserId, p_Body.Endpoint, p_Body.P256dh, p_Body.Auth, p_Body.AllLines), p_CancellationToken);

        /// <summary>Supprime l'abonnement de ce navigateur.</summary>
        [Authorize]
        [HttpPost("unsubscribe")]
        public Task<IActionResult> UnsubscribeAsync([FromBody] PushSubscriptionBody p_Body, CancellationToken p_CancellationToken)
            => SendAsync<RemovePushSubscriptionRequest, bool>(new RemovePushSubscriptionRequest(UserId, p_Body.Endpoint), p_CancellationToken);
    }
}
