using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    /// <summary>
    /// Véhicules rattachés aux lignes du réseau de référence. Course inconnue : confiance abaissée.
    /// Position trop ancienne : marquée obsolète, puis masquée. Avance ou retard et prochain arrêt : prévision
    /// Trip Updates du prochain arrêt de la course (estimation officielle Ametis).
    /// </summary>
    internal class GetVehiclesUseCase(ILogger<GetVehiclesUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IRealtimeStore p_Store, IRealtimeReferenceRepository p_References, IClock p_Clock)
        : ADbUseCase<GetVehiclesRequest, ValueResponse<IReadOnlyList<VehicleView>>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<IReadOnlyList<VehicleView>>> HandleLogic(GetVehiclesRequest p_Request, CancellationToken p_CancellationToken)
        {
            RealtimeSnapshot v_Snapshot = await p_Store.GetAsync(p_CancellationToken);
            DateTime v_Now = p_Clock.UtcNow;
            List<VehiclePosition> v_Visible = (v_Snapshot?.Vehicles ?? [])
                .Where(p_V => RealtimeRules.AgeSeconds(v_Now, p_V.TimestampUtc) <= RealtimeRules.HideAfterSeconds)
                .ToList();
            if (v_Visible.Count == 0)
                return new(Array.Empty<VehicleView>());

            // Prochain arrêt de chaque course : premier, dans l'ordre du flux, dont l'heure effective n'est pas dépassée
            // (même règle que le détail de la course).
            Dictionary<string, StopTimePrediction> v_NextByTrip = v_Snapshot.Predictions
                .Where(p_P => (p_P.DepartureUtc ?? p_P.ArrivalUtc) is { } v_T && !RealtimeRules.IsPassed(v_T, v_Now))
                .GroupBy(p_P => p_P.TripId)
                .ToDictionary(p_G => p_G.Key, p_G => p_G.First());

            (IReadOnlyList<RealtimeLineMatch> v_Lines, IReadOnlyList<RealtimeTripInfo> v_Trips, IReadOnlyDictionary<string, string> v_StopNames) = await WithDatabaseAsync(async () => (
                await p_References.GetLineMatchesAsync(p_CancellationToken),
                await p_References.GetTripsAsync(v_Visible.Where(p_V => p_V.TripId != null).Select(p_V => p_V.TripId).Distinct().ToList(), p_CancellationToken),
                await p_References.GetRealtimeStopNamesAsync(v_NextByTrip.Values.Where(p_P => p_P.StopId != null).Select(p_P => p_P.StopId).Distinct().ToList(), p_CancellationToken)),
                p_CancellationToken);

            Dictionary<string, RealtimeLineMatch> v_LineByRoute = v_Lines.ToDictionary(p_L => p_L.RealtimeRouteId);
            Dictionary<string, RealtimeTripInfo> v_TripById = v_Trips.ToDictionary(p_T => p_T.TripId);
            Dictionary<string, VehicleRouteState> v_RouteStates = (v_Snapshot.RouteStates ?? []).ToDictionary(p_S => p_S.VehicleId);

            return new(v_Visible.Select(p_V =>
            {
                (RealtimeLineMatch v_Line, RealtimeTripInfo v_Trip, MatchConfidence v_Confidence) = RealtimeLinks.Resolve(p_V, v_TripById, v_LineByRoute);
                StopTimePrediction v_Next = p_V.TripId is null ? null : v_NextByTrip.GetValueOrDefault(p_V.TripId);
                VehicleRouteState v_State = v_RouteStates.GetValueOrDefault(p_V.VehicleId);
                bool v_OffRoute = DeviationDetector.IsOffRoute(v_State);
                int v_Age = RealtimeRules.AgeSeconds(v_Now, p_V.TimestampUtc);
                return new VehicleView(p_V.VehicleId, p_V.Label, v_Line?.Line, v_Trip?.Headsign, v_Trip?.DirectionId,
                    p_V.Latitude, p_V.Longitude, p_V.Bearing, p_V.SpeedMetersPerSecond * 3.6, p_V.TimestampUtc,
                    v_Age, v_Age > RealtimeRules.StaleAfterSeconds, v_Confidence,
                    v_Next?.DelaySeconds,
                    v_Next?.StopId is { } v_StopId ? v_StopNames.GetValueOrDefault(v_StopId) : null,
                    v_OffRoute,
                    v_State?.DistanceMeters,
                    v_OffRoute ? v_State.Track : [],
                    p_V.TripId);
            }).ToList());
        }
    }
}
