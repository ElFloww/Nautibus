using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Perturbations publiées (Service Alerts Ametis).</summary>
    [ApiVersion("1.0")]
    public class AlertController(ILogger<AlertController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        /// <summary>Perturbations en cours.</summary>
        [HttpGet]
        public Task<IActionResult> ListAsync(CancellationToken p_CancellationToken)
            => SendAsync<GetAlertsRequest, IReadOnlyList<ServiceAlert>>(new GetAlertsRequest(), p_CancellationToken);
    }
}
