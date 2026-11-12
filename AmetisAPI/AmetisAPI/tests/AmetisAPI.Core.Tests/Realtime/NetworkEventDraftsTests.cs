using AmetisAPI.Core.Realtime;
using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Tests.Realtime
{
    public class NetworkEventDraftsTests
    {
        private static readonly LineSummary m_N1 = new("AMI:N1-1", "N1", "Etouvie <> Pôle Jules Verne", "2880C0", "FFFFFF", 10);

        private static VehicleRouteState State(string p_Vehicle, int p_Count) => new(p_Vehicle, m_N1.Id, p_Count, DateTime.UtcNow, 300, []);

        [Fact]
        public void One_Bus_Off_Route_Gives_A_Low_Confidence_Estimated_Detour()
        {
            NetworkEventDraft v_Draft = NetworkEventDrafts.FromDeviations([State("A", 3), State("B", 1)], new Dictionary<string, LineSummary> { [m_N1.Id] = m_N1 }).Single();

            Check.That(v_Draft.ExternalId).IsEqualTo("detour:AMI:N1-1");
            Check.That(v_Draft.Title).IsEqualTo("Déviation probable – ligne N1");
            Check.That(v_Draft.Confidence).IsEqualTo(MatchConfidence.Low);
            Check.That(v_Draft.Description).Contains("non confirmée par Ametis");
        }

        [Fact]
        public void Two_Buses_Off_Route_Raise_Confidence_To_Medium()
        {
            NetworkEventDraft v_Draft = NetworkEventDrafts.FromDeviations([State("A", 3), State("B", 4)], new Dictionary<string, LineSummary>()).Single();

            Check.That(v_Draft.Confidence).IsEqualTo(MatchConfidence.Medium);
            Check.That(v_Draft.Description).StartsWith("2 bus circulent");
        }

        [Fact]
        public void Official_Alert_Lines_Are_Translated_To_Network_Lines()
        {
            ServiceAlert v_Alert = new("42", "Travaux rue Jules Barni", "Arrêt reporté", null, null, null, ["N1", "INCONNUE"], []);
            Dictionary<string, RealtimeLineMatch> v_Lines = new() { ["N1"] = new RealtimeLineMatch("N1", m_N1, MatchConfidence.High) };

            NetworkEventDraft v_Draft = NetworkEventDrafts.FromAlerts([v_Alert], v_Lines).Single();

            Check.That(v_Draft.ExternalId).IsEqualTo("alert:42");
            Check.That(v_Draft.LineIds).ContainsExactly("AMI:N1-1", "INCONNUE");
            Check.That(v_Draft.Confidence).IsEqualTo(MatchConfidence.High);
        }
    }
}
