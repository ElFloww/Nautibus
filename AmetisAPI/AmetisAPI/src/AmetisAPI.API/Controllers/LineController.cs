using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Lignes du réseau (GTFS complet actif).</summary>
    [ApiVersion("1.0")]
    public class LineController(ILogger<LineController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        /// <summary>Recherche de lignes par nom court ou long ; sans critère, toutes les lignes.</summary>
        [HttpGet]
        public Task<IActionResult> SearchAsync([FromQuery(Name = "q")] string p_Query, CancellationToken p_CancellationToken)
            => SendAsync<SearchLinesRequest, IReadOnlyList<LineSummary>>(new SearchLinesRequest(p_Query), p_CancellationToken);

        /// <summary>Détail d'une ligne : directions et parcours types.</summary>
        [HttpGet("{id}")]
        public Task<IActionResult> GetAsync([FromRoute(Name = "id")] string p_Id, CancellationToken p_CancellationToken)
            => SendAsync<GetLineRequest, LineDetail>(new GetLineRequest(p_Id), p_CancellationToken);

        /// <summary>Tracés de la ligne (coordonnées [longitude, latitude]).</summary>
        [HttpGet("{id}/shapes")]
        public Task<IActionResult> GetShapesAsync([FromRoute(Name = "id")] string p_Id, CancellationToken p_CancellationToken)
            => SendAsync<GetLineShapesRequest, IReadOnlyList<LineShape>>(new GetLineShapesRequest(p_Id), p_CancellationToken);
    }
}
