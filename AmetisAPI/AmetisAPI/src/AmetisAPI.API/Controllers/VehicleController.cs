using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Positions des véhicules et état du temps réel.</summary>
    [ApiVersion("1.0")]
    public class VehicleController(ILogger<VehicleController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        /// <summary>Véhicules visibles (les positions trop anciennes sont masquées).</summary>
        [HttpGet]
        public Task<IActionResult> ListAsync(CancellationToken p_CancellationToken)
            => SendAsync<GetVehiclesRequest, IReadOnlyList<VehicleView>>(new GetVehiclesRequest(), p_CancellationToken);

        /// <summary>Fraîcheur et disponibilité du temps réel.</summary>
        [HttpGet("status")]
        public Task<IActionResult> StatusAsync(CancellationToken p_CancellationToken)
            => SendAsync<GetRealtimeStatusRequest, RealtimeStatus>(new GetRealtimeStatusRequest(), p_CancellationToken);
    }
}
