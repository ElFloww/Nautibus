using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Courses suivies en temps réel.</summary>
    [ApiVersion("1.0")]
    public class TripController(ILogger<TripController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        /// <summary>Arrêts desservis et à venir d'une course (heure prévue, heure effective, écart), position et trace du bus.</summary>
        [HttpGet("{id}")]
        public Task<IActionResult> GetAsync([FromRoute(Name = "id")] string p_Id, CancellationToken p_CancellationToken)
            => SendAsync<GetTripRequest, TripDetail>(new GetTripRequest(p_Id), p_CancellationToken);
    }
}
