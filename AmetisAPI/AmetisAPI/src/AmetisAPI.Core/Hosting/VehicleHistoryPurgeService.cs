using AmetisAPI.Core.Requests.Realtime;
using Microsoft.Extensions.DependencyInjection;

namespace AmetisAPI.Core.Hosting
{
    /// <summary>Purge quotidienne de l'historique des positions au-delà de 30 jours.</summary>
    public class VehicleHistoryPurgeService(IServiceScopeFactory p_ScopeFactory, ILogger<VehicleHistoryPurgeService> p_Logger)
        : ScheduledMediatorService(p_ScopeFactory, p_Logger)
    {
        protected override TimeSpan Interval => TimeSpan.FromDays(1);

        protected internal override Task RunOnceAsync(IMediator p_Mediator, CancellationToken p_CancellationToken)
        {
            return p_Mediator.SendQueryAsync<PurgeVehicleHistoryRequest, ValueResponse<int>>(new PurgeVehicleHistoryRequest(), p_CancellationToken);
        }
    }
}
