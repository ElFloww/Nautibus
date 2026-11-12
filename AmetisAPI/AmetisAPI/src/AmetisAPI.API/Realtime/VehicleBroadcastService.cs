using AmetisAPI.Core.Requests.Events;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace AmetisAPI.API.Realtime
{
    /// <summary>Surveille l'état temps réel et pousse les véhicules aux clients SignalR à chaque nouvelle collecte.</summary>
    internal class VehicleBroadcastService(IServiceScopeFactory p_ScopeFactory, IHubContext<VehicleHub> p_Hub, ILogger<VehicleBroadcastService> p_Logger)
        : AmetisAPI.Core.Hosting.ScheduledMediatorService(p_ScopeFactory, p_Logger)
    {
        private DateTime? m_LastCollect;

        /// <inheritdoc />
        protected override TimeSpan Interval => TimeSpan.FromSeconds(5);

        /// <inheritdoc />
        protected override async Task RunOnceAsync(IMediator p_Mediator, CancellationToken p_CancellationToken)
        {
            ValueResponse<RealtimeStatus> v_Status = await p_Mediator.SendQueryAsync<GetRealtimeStatusRequest, ValueResponse<RealtimeStatus>>(new GetRealtimeStatusRequest(), p_CancellationToken);
            if (!v_Status.Success || v_Status.Value.CollectedAtUtc is null || v_Status.Value.CollectedAtUtc == m_LastCollect)
                return;

            ValueResponse<IReadOnlyList<VehicleView>> v_Vehicles = await p_Mediator.SendQueryAsync<GetVehiclesRequest, ValueResponse<IReadOnlyList<VehicleView>>>(new GetVehiclesRequest(), p_CancellationToken);
            if (!v_Vehicles.Success)
                return;
            m_LastCollect = v_Status.Value.CollectedAtUtc;
            await p_Hub.Clients.All.SendAsync(VehicleHub.VehiclesEvent, v_Vehicles.Value, p_CancellationToken);

            ValueResponse<IReadOnlyList<NetworkEventView>> v_Events = await p_Mediator.SendQueryAsync<ListNetworkEventsRequest, ValueResponse<IReadOnlyList<NetworkEventView>>>(new ListNetworkEventsRequest(), p_CancellationToken);
            if (v_Events.Success)
                await p_Hub.Clients.All.SendAsync(VehicleHub.EventsEvent, v_Events.Value, p_CancellationToken);
        }
    }
}
