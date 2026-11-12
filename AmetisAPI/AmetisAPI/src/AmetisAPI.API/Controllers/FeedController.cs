using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.API.Controllers
{
    /// <summary>Journal des imports et état des données (information publique, sans donnée personnelle).</summary>
    [ApiVersion("1.0")]
    public class FeedController(ILogger<FeedController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
    {
        /// <summary>Derniers imports GTFS.</summary>
        [HttpGet]
        public Task<IActionResult> ListAsync([FromQuery(Name = "count")] int p_Count = 20, CancellationToken p_CancellationToken = default)
            => SendAsync<ListFeedVersionsRequest, IReadOnlyList<FeedVersionInfo>>(new ListFeedVersionsRequest(p_Count), p_CancellationToken);

        /// <summary>État de santé des données.</summary>
        [HttpGet("health")]
        public Task<IActionResult> HealthAsync(CancellationToken p_CancellationToken)
            => SendAsync<GetDataHealthRequest, DataHealth>(new GetDataHealthRequest(), p_CancellationToken);
    }
}
