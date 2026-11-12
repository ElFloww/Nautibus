using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Realtime
{
    /// <summary>Rattachement d'un véhicule à sa course et à sa ligne du réseau, avec la confiance du lien le plus faible.</summary>
    internal static class RealtimeLinks
    {
        public static (RealtimeLineMatch Line, RealtimeTripInfo Trip, MatchConfidence Confidence) Resolve(
            VehiclePosition p_Vehicle, IReadOnlyDictionary<string, RealtimeTripInfo> p_Trips, IReadOnlyDictionary<string, RealtimeLineMatch> p_Lines)
        {
            RealtimeTripInfo v_Trip = p_Vehicle.TripId is null ? null : p_Trips.GetValueOrDefault(p_Vehicle.TripId);
            string v_RouteId = p_Vehicle.RouteId ?? v_Trip?.RouteId;
            RealtimeLineMatch v_Line = v_RouteId is null ? null : p_Lines.GetValueOrDefault(v_RouteId);
            MatchConfidence v_Confidence = v_Line?.Confidence ?? MatchConfidence.None;
            if (v_Trip is null)
                v_Confidence = RealtimeRules.Min(v_Confidence, MatchConfidence.Low);
            return (v_Line, v_Trip, v_Confidence);
        }
    }
}
