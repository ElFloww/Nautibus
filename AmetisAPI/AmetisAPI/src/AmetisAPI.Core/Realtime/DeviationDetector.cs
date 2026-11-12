using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Realtime
{
    /// <summary>
    /// Détection estimée des déviations : un bus rattaché avec une confiance au moins moyenne, dont plusieurs positions
    /// successives (horodatages distincts) sont à plus de <see cref="OffRouteMeters"/> m de tous les tracés de sa ligne,
    /// est considéré hors itinéraire. Ce n'est jamais une information officielle.
    /// </summary>
    internal static class DeviationDetector
    {
        public const double OffRouteMeters = 150;
        public const int ConsecutivePositions = 3;
        public const int MaxTrackPoints = 40;

        public static bool IsOffRoute(VehicleRouteState p_State) => p_State is not null && p_State.OffRouteCount >= ConsecutivePositions;

        public static IReadOnlyList<VehicleRouteState> Update(
            IReadOnlyList<VehicleRouteState> p_Previous,
            IEnumerable<(VehiclePosition Vehicle, string LineId, MatchConfidence Confidence)> p_Vehicles,
            IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>> p_ShapesByLine,
            DateTime p_NowUtc)
        {
            Dictionary<string, VehicleRouteState> v_Previous = (p_Previous ?? []).ToDictionary(p_S => p_S.VehicleId);
            List<VehicleRouteState> v_States = [];
            foreach ((VehiclePosition v_Vehicle, string v_LineId, MatchConfidence v_Confidence) in p_Vehicles)
            {
                if (v_LineId is null || v_Confidence < MatchConfidence.Medium
                    || RealtimeRules.AgeSeconds(p_NowUtc, v_Vehicle.TimestampUtc) > RealtimeRules.StaleAfterSeconds
                    || !p_ShapesByLine.TryGetValue(v_LineId, out IReadOnlyList<IReadOnlyList<double[]>> v_Shapes) || v_Shapes.Count == 0)
                    continue;

                VehicleRouteState v_Old = v_Previous.GetValueOrDefault(v_Vehicle.VehicleId);
                if (v_Old is not null && v_Old.LineId != v_LineId)
                    v_Old = null; // changement de ligne : on repart de zéro
                if (v_Old is not null && v_Old.LastTimestampUtc == v_Vehicle.TimestampUtc)
                {
                    v_States.Add(v_Old); // même position que la collecte précédente : rien de nouveau
                    continue;
                }

                double v_Distance = GeoMath.DistanceToShapesMeters(v_Vehicle.Latitude, v_Vehicle.Longitude, v_Shapes);
                bool v_Off = v_Distance > OffRouteMeters;
                IReadOnlyList<double[]> v_Track = v_Off
                    ? [.. (v_Old?.Track ?? []).TakeLast(MaxTrackPoints - 1), new[] { v_Vehicle.Longitude, v_Vehicle.Latitude }]
                    : [];
                v_States.Add(new VehicleRouteState(v_Vehicle.VehicleId, v_LineId, v_Off ? (v_Old?.OffRouteCount ?? 0) + 1 : 0, v_Vehicle.TimestampUtc, v_Distance, v_Track));
            }
            return v_States;
        }
    }
}
