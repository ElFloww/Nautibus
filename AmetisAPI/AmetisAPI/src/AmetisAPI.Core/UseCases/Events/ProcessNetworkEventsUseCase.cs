using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Events;
using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Push;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Events;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Push;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Events
{
    /// <summary>
    /// Met à jour le suivi des bus par rapport à leur tracé, rapproche alertes officielles et déviations détectées des
    /// évènements enregistrés, puis notifie les abonnés des évènements nouveaux.
    /// </summary>
    internal class ProcessNetworkEventsUseCase(
        ILogger<ProcessNetworkEventsUseCase> p_Logger,
        IUnitOfWork p_UnitOfWork,
        IRealtimeStore p_Store,
        IRealtimeReferenceRepository p_References,
        INetworkShapeRepository p_Shapes,
        INetworkRepository p_Network,
        INetworkEventRepository p_Events,
        IPushSubscriptionRepository p_Subscriptions,
        IPushSender p_Push,
        IClock p_Clock)
        : ADbUseCase<ProcessNetworkEventsRequest, ValueResponse<int>>(p_Logger, p_UnitOfWork)
    {
        /// <summary>Une déviation non revue pendant ce délai est considérée terminée.</summary>
        public static readonly TimeSpan m_DetourCloseAfter = TimeSpan.FromMinutes(10);

        protected override async Task<ValueResponse<int>> HandleLogic(ProcessNetworkEventsRequest p_Request, CancellationToken p_CancellationToken)
        {
            RealtimeSnapshot v_Snapshot = await p_Store.GetAsync(p_CancellationToken);
            if (v_Snapshot is null)
                return new(0);
            DateTime v_Now = p_Clock.UtcNow;

            List<NetworkEvent> v_Created = await WithDatabaseAsync(async () =>
            {
                Dictionary<string, RealtimeLineMatch> v_Lines = (await p_References.GetLineMatchesAsync(p_CancellationToken)).ToDictionary(p_L => p_L.RealtimeRouteId);
                Dictionary<string, RealtimeTripInfo> v_Trips = (await p_References.GetTripsAsync(
                    v_Snapshot.Vehicles.Where(p_V => p_V.TripId != null).Select(p_V => p_V.TripId).Distinct().ToList(), p_CancellationToken)).ToDictionary(p_T => p_T.TripId);
                IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>> v_Shapes = await p_Shapes.GetShapesByLineAsync(p_CancellationToken);
                Dictionary<string, LineSummary> v_LinesById = (await p_Network.SearchLinesAsync(null, p_CancellationToken)).ToDictionary(p_L => p_L.Id);

                IReadOnlyList<VehicleRouteState> v_States = DeviationDetector.Update(v_Snapshot.RouteStates, v_Snapshot.Vehicles.Select(p_V =>
                {
                    (RealtimeLineMatch v_Line, _, MatchConfidence v_Confidence) = RealtimeLinks.Resolve(p_V, v_Trips, v_Lines);
                    return (p_V, v_Line?.Line.Id, v_Confidence);
                }), v_Shapes, v_Now);
                await p_Store.SaveAsync(v_Snapshot with { RouteStates = v_States }, p_CancellationToken);

                IReadOnlyList<NetworkEvent> v_Official = await p_Events.SyncAsync(NetworkEventKind.Official,
                    NetworkEventDrafts.FromAlerts(v_Snapshot.Alerts, v_Lines), v_Now, TimeSpan.Zero, p_CancellationToken);
                IReadOnlyList<NetworkEvent> v_Detected = await p_Events.SyncAsync(NetworkEventKind.DetectedDetour,
                    NetworkEventDrafts.FromDeviations(v_States, v_LinesById), v_Now, m_DetourCloseAfter, p_CancellationToken);
                return v_Official.Concat(v_Detected).ToList();
            }, p_CancellationToken);

            foreach (NetworkEvent v_Event in v_Created)
                await NotifyAsync(v_Event, p_CancellationToken);
            if (v_Created.Count > 0)
                m_Logger.LogInformation("Évènements réseau : {Count} nouveau(x)", v_Created.Count);
            return new(v_Created.Count);
        }

        private async Task NotifyAsync(NetworkEvent p_Event, CancellationToken p_CancellationToken)
        {
            if (!p_Push.IsEnabled)
                return;
            IReadOnlyList<PushSubscriptionInfo> v_Targets = await WithDatabaseAsync(() => p_Subscriptions.ListForLinesAsync(p_Event.LineIds, p_CancellationToken), p_CancellationToken);
            PushMessage v_Message = new(p_Event.Title, p_Event.Description ?? "", "/infos-trafic", $"event-{p_Event.Id}");
            int v_Sent = 0, v_Failed = 0;
            foreach (PushSubscriptionInfo v_Target in v_Targets)
            {
                PushResult v_Result = await p_Push.SendAsync(v_Target, v_Message, p_CancellationToken);
                if (v_Result == PushResult.Gone)
                    await WithDatabaseAsync(async () => { await p_Subscriptions.RemoveByEndpointAsync(v_Target.Endpoint, p_CancellationToken); return true; }, p_CancellationToken);
                if (v_Result == PushResult.Sent) v_Sent++;
                else if (v_Result == PushResult.Failed) v_Failed++;
            }
            m_Logger.LogInformation("Notification de l'évènement {EventId} : {Sent} envoyée(s), {Failed} échec(s)", p_Event.Id, v_Sent, v_Failed);
        }
    }
}
