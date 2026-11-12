using AmetisAPI.Core.Realtime;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Tests.Realtime
{
    public class StopPassageExtractorTests
    {
        private static readonly DateTime m_T = new(2026, 11, 12, 13, 30, 42, DateTimeKind.Utc);

        [Fact]
        public void Scheduled_Time_Is_Published_Time_Minus_Delay_And_Order_Follows_The_Feed()
        {
            // Valeurs relevées dans le flux Ametis : 13:30:42 avec 222 s de retard = 13:27:00 prévu au GTFS.
            IReadOnlyList<StopPassage> v_Passages = StopPassageExtractor.Extract(
            [
                new StopTimePrediction("T1", "11", "655", null, null, m_T, 222, "822"),
                new StopTimePrediction("T1", "11", "1214", null, null, m_T.AddMinutes(2), 249, "822"),
                new StopTimePrediction("T2", "8", "40", null, null, m_T, -30, "10"),
                new StopTimePrediction("T1", "11", "999", null, null, m_T.AddMinutes(3), null, "822")
            ]);

            Check.That(v_Passages).HasSize(3);
            Check.That(v_Passages[0].ScheduledUtc).IsEqualTo(new DateTime(2026, 11, 12, 13, 27, 0, DateTimeKind.Utc));
            Check.That(v_Passages[1].StopOrder).IsEqualTo(2);
            Check.That(v_Passages.Single(p_P => p_P.TripId == "T2").ScheduledUtc).IsEqualTo(m_T.AddSeconds(30));
            Check.That(v_Passages.Any(p_P => p_P.StopId == "999")).IsFalse(); // sans retard publié : heure prévue inconnue
        }
    }
}
