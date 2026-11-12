using AmetisAPI.Core.Requests.Events;
using AmetisAPI.Domain.Bo.Events;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Infos trafic : alertes officielles Ametis et déviations détectées (estimations).</summary>
    [ApiVersion("1.0")]
    public class EventController(ILogger<EventController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        /// <summary>Évènements en cours et terminés depuis le nombre de jours demandé.</summary>
        [HttpGet]
        public Task<IActionResult> ListAsync([FromQuery(Name = "days")] int p_Days = 7, CancellationToken p_CancellationToken = default)
            => SendAsync<ListNetworkEventsRequest, IReadOnlyList<NetworkEventView>>(new ListNetworkEventsRequest(p_Days), p_CancellationToken);
    }
}
