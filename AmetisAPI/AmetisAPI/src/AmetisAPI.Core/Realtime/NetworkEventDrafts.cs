using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Realtime
{
    /// <summary>Construction des évènements à partir des alertes officielles et des déviations détectées.</summary>
    internal static class NetworkEventDrafts
    {
        public const string DetourPrefix = "detour:";
        public const string AlertPrefix = "alert:";

        public static IReadOnlyList<NetworkEventDraft> FromAlerts(IReadOnlyList<ServiceAlert> p_Alerts, IReadOnlyDictionary<string, RealtimeLineMatch> p_Lines)
        {
            return p_Alerts.Select((p_A, p_Index) => new NetworkEventDraft(
                    AlertPrefix + (p_A.Id ?? $"{p_A.Header}|{p_A.StartUtc:o}|{p_Index}"),
                    string.IsNullOrWhiteSpace(p_A.Header) ? "Perturbation" : p_A.Header,
                    p_A.Description,
                    p_A.Url,
                    p_A.RouteIds.Select(p_R => p_Lines.GetValueOrDefault(p_R)?.Line.Id ?? p_R).Distinct().ToList(),
                    p_A.StartUtc,
                    p_A.EndUtc,
                    MatchConfidence.High))
                .ToList();
        }

        public static IReadOnlyList<NetworkEventDraft> FromDeviations(IReadOnlyList<VehicleRouteState> p_States, IReadOnlyDictionary<string, LineSummary> p_LinesById)
        {
            return p_States.Where(DeviationDetector.IsOffRoute)
                .GroupBy(p_S => p_S.LineId)
                .Select(p_G =>
                {
                    int v_Count = p_G.Count();
                    string v_Name = p_LinesById.GetValueOrDefault(p_G.Key)?.ShortName ?? p_G.Key;
                    // Plusieurs bus hors tracé, ou un bus durablement hors tracé : déviation plus probable qu'un détour isolé.
                    MatchConfidence v_Confidence = v_Count >= 2 || p_G.Any(p_S => p_S.OffRouteCount >= DeviationDetector.ConsecutivePositions * 2)
                        ? MatchConfidence.Medium : MatchConfidence.Low;
                    return new NetworkEventDraft(
                        DetourPrefix + p_G.Key,
                        $"Déviation probable – ligne {v_Name}",
                        $"{v_Count} bus circule{(v_Count > 1 ? "nt" : "")} à plus de {DeviationDetector.OffRouteMeters:0} m de l'itinéraire habituel. "
                        + "Information estimée à partir des positions GPS des bus, non confirmée par Ametis.",
                        null,
                        [p_G.Key],
                        null,
                        null,
                        v_Confidence);
                })
                .ToList();
        }

        public static NetworkEventView ToView(NetworkEvent p_Event, IReadOnlyDictionary<string, LineSummary> p_LinesById, DateTime p_NowUtc)
        {
            return new NetworkEventView(p_Event.Id, p_Event.Kind, p_Event.Title, p_Event.Description, p_Event.Url,
                p_Event.LineIds.Select(p_Id => p_LinesById.GetValueOrDefault(p_Id)).Where(p_L => p_L is not null).ToList(),
                p_Event.StartUtc, p_Event.EndUtc, p_Event.ClosedUtc, p_Event.LastSeenUtc, p_Event.Confidence, p_Event.IsActiveAt(p_NowUtc));
        }
    }
}
