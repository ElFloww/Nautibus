using AmetisAPI.Core.Network;
using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    /// <summary>
    /// Prochains passages temps réel d'un arrêt, tirés des Trip Updates Ametis. Les arrêts du flux temps réel sont
    /// rattachés aux arrêts du réseau par la table de correspondance ; la confiance retenue est la plus faible des deux liens.
    /// </summary>
    internal class GetStopRealtimeUseCase(
        ILogger<GetStopRealtimeUseCase> p_Logger,
        IUnitOfWork p_UnitOfWork,
        IRealtimeStore p_Store,
        INetworkRepository p_Network,
        IRealtimeReferenceRepository p_References,
        IClock p_Clock)
        : ADbUseCase<GetStopRealtimeRequest, ValueResponse<IReadOnlyList<RealtimeArrival>>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<IReadOnlyList<RealtimeArrival>>> HandleLogic(GetStopRealtimeRequest p_Request, CancellationToken p_CancellationToken)
        {
            StopDetail v_Stop = await WithDatabaseAsync(() => p_Network.GetStopAsync(p_Request.StopId, p_CancellationToken), p_CancellationToken);
            if (v_Stop is null)
                return ValueResponse<IReadOnlyList<RealtimeArrival>>.Error(DomainErrors.Data.NotFound, "Arrêt non trouvé");

            RealtimeSnapshot v_Snapshot = await p_Store.GetAsync(p_CancellationToken);
            if (v_Snapshot is null || v_Snapshot.Predictions.Count == 0)
                return new(Array.Empty<RealtimeArrival>());

            DateTime v_Now = p_Clock.UtcNow;
            List<string> v_NetworkStopIds = [v_Stop.Stop.Id, .. v_Stop.Quays.Select(p_Q => p_Q.Id)];

            IReadOnlyList<RealtimeStopMatch> v_StopMatches = await WithDatabaseAsync(() => p_References.GetStopMatchesForNetworkStopsAsync(v_NetworkStopIds, p_CancellationToken), p_CancellationToken);
            Dictionary<string, RealtimeStopMatch> v_MatchByRtStop = v_StopMatches.ToDictionary(p_M => p_M.RealtimeStopId);

            List<StopTimePrediction> v_Predictions = v_Snapshot.Predictions
                .Where(p_P => p_P.StopId is not null && v_MatchByRtStop.ContainsKey(p_P.StopId))
                .Where(p_P => (p_P.DepartureUtc ?? p_P.ArrivalUtc) is { } v_T && (v_T - v_Now).TotalSeconds >= -RealtimeRules.PastArrivalToleranceSeconds)
                .ToList();
            if (v_Predictions.Count == 0)
                return new(Array.Empty<RealtimeArrival>());

            (IReadOnlyList<RealtimeLineMatch> v_Lines, IReadOnlyList<RealtimeTripInfo> v_Trips) = await WithDatabaseAsync(async () => (
                await p_References.GetLineMatchesAsync(p_CancellationToken),
                await p_References.GetTripsAsync(v_Predictions.Select(p_P => p_P.TripId).Distinct().ToList(), p_CancellationToken)),
                p_CancellationToken);
            Dictionary<string, RealtimeLineMatch> v_LineByRoute = v_Lines.ToDictionary(p_L => p_L.RealtimeRouteId);
            Dictionary<string, RealtimeTripInfo> v_TripById = v_Trips.ToDictionary(p_T => p_T.TripId);

            int v_FeedAge = v_Snapshot.FeedTimestampUtc is { } v_Ts ? RealtimeRules.AgeSeconds(v_Now, v_Ts) : int.MaxValue;
            bool v_Stale = v_FeedAge > RealtimeRules.StaleAfterSeconds;

            return new(v_Predictions.Select(p_P =>
            {
                RealtimeTripInfo v_Trip = v_TripById.GetValueOrDefault(p_P.TripId);
                RealtimeLineMatch v_Line = (p_P.RouteId ?? v_Trip?.RouteId) is { } v_RouteId ? v_LineByRoute.GetValueOrDefault(v_RouteId) : null;
                RealtimeStopMatch v_StopMatch = v_MatchByRtStop[p_P.StopId];
                MatchConfidence v_Confidence = RealtimeRules.Min(v_StopMatch.Confidence, v_Line?.Confidence ?? MatchConfidence.None);
                if (v_Trip is null)
                    v_Confidence = RealtimeRules.Min(v_Confidence, MatchConfidence.Low);
                return new RealtimeArrival(p_P.TripId, v_StopMatch.NetworkStopId, v_Line?.Line, v_Trip?.Headsign,
                    NetworkTime.ToParis((p_P.DepartureUtc ?? p_P.ArrivalUtc).Value), p_P.DelaySeconds,
                    RealtimeRules.TripUpdatesProvenance, v_Confidence, v_FeedAge, v_Stale);
            })
            .OrderBy(p_A => p_A.ExpectedTime)
            .Take(p_Request.Limit)
            .ToList());
        }
    }
}
