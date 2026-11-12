using AmetisAPI.Core.Network;
using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    /// <summary>
    /// Détail d'une course : chaque arrêt avec heure prévue, heure effective publiée par Ametis et écart. Un arrêt est
    /// « desservi » quand son heure effective est passée ; le premier non desservi est le prochain arrêt.
    /// </summary>
    internal class GetTripUseCase(
        ILogger<GetTripUseCase> p_Logger,
        IUnitOfWork p_UnitOfWork,
        IStopPassageRepository p_Passages,
        IRealtimeReferenceRepository p_References,
        IVehicleHistoryRepository p_History,
        IRealtimeStore p_Store,
        IClock p_Clock)
        : ADbUseCase<GetTripRequest, ValueResponse<TripDetail>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<TripDetail>> HandleLogic(GetTripRequest p_Request, CancellationToken p_CancellationToken)
        {
            DateTime v_Now = p_Clock.UtcNow;
            RealtimeSnapshot v_Snapshot = await p_Store.GetAsync(p_CancellationToken);

            return await WithDatabaseAsync(async () =>
            {
                IReadOnlyList<StopPassage> v_Passages = await p_Passages.ListForTripAsync(p_Request.TripId, v_Now, p_CancellationToken);
                if (v_Passages.Count == 0)
                    return ValueResponse<TripDetail>.Error(DomainErrors.Data.NotFound, "Course inconnue ou sans données temps réel");

                List<string> v_StopIds = v_Passages.Select(p_P => p_P.StopId).Distinct().ToList();
                Dictionary<string, RealtimeStopInfo> v_StopInfos = (await p_References.GetRealtimeStopsAsync(v_StopIds, p_CancellationToken)).ToDictionary(p_S => p_S.StopId);
                IReadOnlyDictionary<string, string> v_NetworkStops = await p_References.GetNetworkStopsForRealtimeStopsAsync(v_StopIds, p_CancellationToken);
                Dictionary<string, RealtimeTripInfo> v_Trips = (await p_References.GetTripsAsync([p_Request.TripId], p_CancellationToken)).ToDictionary(p_T => p_T.TripId);
                Dictionary<string, RealtimeLineMatch> v_Lines = (await p_References.GetLineMatchesAsync(p_CancellationToken)).ToDictionary(p_L => p_L.RealtimeRouteId);

                VehiclePosition v_Vehicle = v_Snapshot?.Vehicles.Where(p_V => p_V.TripId == p_Request.TripId).MaxBy(p_V => p_V.TimestampUtc);
                VehiclePosition v_Reference = v_Vehicle ?? new VehiclePosition(v_Passages[0].VehicleId, null, p_Request.TripId, v_Passages[0].RouteId, 0, 0, null, null, null, v_Now);
                (RealtimeLineMatch v_Line, RealtimeTripInfo v_Trip, MatchConfidence v_Confidence) = RealtimeLinks.Resolve(v_Reference, v_Trips, v_Lines);

                IReadOnlyList<double[]> v_Trace = await p_History.GetTripTraceAsync(p_Request.TripId,
                    v_Passages.Min(p_P => p_P.ScheduledUtc).AddMinutes(-15), v_Passages.Max(p_P => p_P.ExpectedUtc).AddMinutes(15), p_CancellationToken);

                int v_NextIndex = v_Passages.ToList().FindIndex(p_P => !RealtimeRules.IsPassed(p_P.ExpectedUtc, v_Now));
                List<TripStopView> v_Stops = v_Passages.Select((p_P, p_I) => new TripStopView(
                    p_P.StopOrder,
                    v_StopInfos.GetValueOrDefault(p_P.StopId)?.Name ?? p_P.StopId,
                    v_NetworkStops.GetValueOrDefault(p_P.StopId),
                    NetworkTime.ToParis(p_P.ScheduledUtc),
                    NetworkTime.ToParis(p_P.ExpectedUtc),
                    p_P.DelaySeconds,
                    v_NextIndex < 0 || p_I < v_NextIndex ? TripStopStatus.Passed : p_I == v_NextIndex ? TripStopStatus.Next : TripStopStatus.Upcoming,
                    v_StopInfos.GetValueOrDefault(p_P.StopId)?.Latitude,
                    v_StopInfos.GetValueOrDefault(p_P.StopId)?.Longitude)).ToList();

                int? v_FeedAge = v_Snapshot?.FeedTimestampUtc is { } v_Ts ? RealtimeRules.AgeSeconds(v_Now, v_Ts) : null;
                int? v_PositionAge = v_Vehicle is null ? null : RealtimeRules.AgeSeconds(v_Now, v_Vehicle.TimestampUtc);
                return new ValueResponse<TripDetail>(new TripDetail(
                    p_Request.TripId, v_Line?.Line, v_Confidence, v_Trip?.Headsign, v_Trip?.DirectionId,
                    v_Vehicle?.VehicleId ?? v_Passages[0].VehicleId, v_Vehicle?.Latitude, v_Vehicle?.Longitude, v_Vehicle?.Bearing, v_PositionAge,
                    v_FeedAge, v_FeedAge is null || v_FeedAge > RealtimeRules.StaleAfterSeconds, v_Stops, v_Trace));
            }, p_CancellationToken);
        }
    }
}
