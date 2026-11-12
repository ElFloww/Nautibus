using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Tests.UseCases.Realtime
{
    internal static class RealtimeTestData
    {
        public static readonly DateTime m_Now = new(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);
        public static readonly LineSummary m_Line = new("AMI:5A-1", "5A", "Ligne 5A", "E30613", "FFFFFF", 5);

        public static VehiclePosition Vehicle(string p_Id, int p_AgeSeconds, string p_TripId = "2-5A-1") =>
            new(p_Id, p_Id, p_TripId, "5A", 49.9, 2.3, 90, 10, null, m_Now.AddSeconds(-p_AgeSeconds));

        public static RealtimeSnapshot Snapshot(IReadOnlyList<VehiclePosition> p_Vehicles, IReadOnlyList<StopTimePrediction> p_Predictions = null, int p_FeedAge = 30) =>
            new(m_Now.AddSeconds(-p_FeedAge), m_Now, p_Vehicles, p_Predictions ?? [], [], [], []);
    }
}
