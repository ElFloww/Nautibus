using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Events;
using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Events;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Events
{
    /// <summary>Évènements des derniers jours, en cours d'abord.</summary>
    internal class ListNetworkEventsUseCase(ILogger<ListNetworkEventsUseCase> p_Logger, IUnitOfWork p_UnitOfWork, INetworkEventRepository p_Events, INetworkRepository p_Network, IClock p_Clock)
        : ADbUseCase<ListNetworkEventsRequest, ValueResponse<IReadOnlyList<NetworkEventView>>>(p_Logger, p_UnitOfWork)
    {
        protected override Task<ValueResponse<IReadOnlyList<NetworkEventView>>> HandleLogic(ListNetworkEventsRequest p_Request, CancellationToken p_CancellationToken)
        {
            return WithDatabaseAsync(async () =>
            {
                DateTime v_Now = p_Clock.UtcNow;
                IReadOnlyList<NetworkEvent> v_Events = await p_Events.ListAsync(v_Now.AddDays(-p_Request.Days), p_CancellationToken);
                Dictionary<string, LineSummary> v_Lines = (await p_Network.SearchLinesAsync(null, p_CancellationToken)).ToDictionary(p_L => p_L.Id);
                return new ValueResponse<IReadOnlyList<NetworkEventView>>(v_Events
                    .Select(p_E => NetworkEventDrafts.ToView(p_E, v_Lines, v_Now))
                    .OrderByDescending(p_E => p_E.IsActive)
                    .ThenByDescending(p_E => p_E.StartUtc)
                    .ToList());
            }, p_CancellationToken);
        }
    }
}
