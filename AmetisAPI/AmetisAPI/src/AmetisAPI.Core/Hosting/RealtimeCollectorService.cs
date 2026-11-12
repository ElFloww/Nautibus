using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using Microsoft.Extensions.DependencyInjection;

namespace AmetisAPI.Core.Hosting
{
    /// <summary>Collecte GTFS-RT toutes les 30 s (flux Ametis rafraîchi toutes les 40 à 60 s, audit P0).</summary>
    public class RealtimeCollectorService(IServiceScopeFactory p_ScopeFactory, ILogger<RealtimeCollectorService> p_Logger)
        : ScheduledMediatorService(p_ScopeFactory, p_Logger)
    {
        protected override TimeSpan Interval => TimeSpan.FromSeconds(30);

        protected override async Task RunOnceAsync(IMediator p_Mediator, CancellationToken p_CancellationToken)
        {
            ValueResponse<RealtimeStatus> v_Result = await p_Mediator.SendQueryAsync<CollectRealtimeRequest, ValueResponse<RealtimeStatus>>(new CollectRealtimeRequest(), p_CancellationToken);
            if (!v_Result.Success)
                m_Logger.LogWarning("Collecte GTFS-RT en échec : {Errors}", string.Join(" ; ", v_Result.Errors.Select(p_E => p_E.Code)));
            else
                await p_Mediator.SendQueryAsync<Requests.Events.ProcessNetworkEventsRequest, ValueResponse<int>>(new Requests.Events.ProcessNetworkEventsRequest(), p_CancellationToken);
            if (v_Result.Success)
                m_Logger.LogDebug("Collecte GTFS-RT : {Vehicles} véhicules, âge du flux {Age} s", v_Result.Value.VehicleCount, v_Result.Value.FeedAgeSeconds);
        }
    }
}
