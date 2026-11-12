using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Arrêts, horaires théoriques et passages temps réel.</summary>
    [ApiVersion("1.0")]
    public class StopController(ILogger<StopController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        /// <summary>Recherche d'arrêts par nom et/ou autour d'un point (rayon en mètres).</summary>
        [HttpGet]
        public Task<IActionResult> SearchAsync([FromQuery(Name = "q")] string p_Query, [FromQuery(Name = "lat")] double? p_Latitude, [FromQuery(Name = "lon")] double? p_Longitude,
            [FromQuery(Name = "radius")] int p_Radius = 500, [FromQuery(Name = "limit")] int p_Limit = 50, CancellationToken p_CancellationToken = default)
            => SendAsync<SearchStopsRequest, IReadOnlyList<StopSummary>>(new SearchStopsRequest(p_Query, p_Latitude, p_Longitude, p_Radius, p_Limit), p_CancellationToken);

        /// <summary>Détail d'un arrêt et de ses quais.</summary>
        [HttpGet("{id}")]
        public Task<IActionResult> GetAsync([FromRoute(Name = "id")] string p_Id, CancellationToken p_CancellationToken)
            => SendAsync<GetStopRequest, StopDetail>(new GetStopRequest(p_Id), p_CancellationToken);

        /// <summary>Prochains passages théoriques.</summary>
        [HttpGet("{id}/departures")]
        public Task<IActionResult> GetDeparturesAsync([FromRoute(Name = "id")] string p_Id, [FromQuery(Name = "from")] DateTimeOffset? p_From, [FromQuery(Name = "limit")] int p_Limit = 20, CancellationToken p_CancellationToken = default)
            => SendAsync<GetDeparturesRequest, IReadOnlyList<Departure>>(new GetDeparturesRequest(p_Id, p_From, p_Limit), p_CancellationToken);

        /// <summary>Tableau de l'arrêt : prochains passages (heure prévue, heure effective et écart) et derniers passages.</summary>
        [HttpGet("{id}/board")]
        public Task<IActionResult> GetBoardAsync([FromRoute(Name = "id")] string p_Id, CancellationToken p_CancellationToken)
            => SendAsync<GetStopBoardRequest, StopBoard>(new GetStopBoardRequest(p_Id), p_CancellationToken);

        /// <summary>Prochains passages temps réel (Trip Updates Ametis), avec provenance et confiance.</summary>
        [HttpGet("{id}/realtime")]
        public Task<IActionResult> GetRealtimeAsync([FromRoute(Name = "id")] string p_Id, CancellationToken p_CancellationToken)
            => SendAsync<GetStopRealtimeRequest, IReadOnlyList<RealtimeArrival>>(new GetStopRealtimeRequest(p_Id), p_CancellationToken);
    }
}
