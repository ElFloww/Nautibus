using AmetisAPI.Core.Realtime;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Tests.Realtime
{
    public class DeviationDetectorTests
    {
        private static readonly DateTime m_Now = new(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>> m_Shapes =
            new Dictionary<string, IReadOnlyList<IReadOnlyList<double[]>>> { ["L1"] = [[[2.30, 49.9], [2.31, 49.9]]] };

        private static VehiclePosition At(double p_Lat, int p_SecondsAgo) => new("B1", "B1", "T1", "1", p_Lat, 2.305, 90, 8, null, m_Now.AddSeconds(-p_SecondsAgo));

        private static IReadOnlyList<VehicleRouteState> Step(IReadOnlyList<VehicleRouteState> p_Previous, VehiclePosition p_Position, MatchConfidence p_Confidence = MatchConfidence.High)
            => DeviationDetector.Update(p_Previous, [(p_Position, "L1", p_Confidence)], m_Shapes, m_Now);

        [Fact]
        public void Three_Successive_Positions_Over_150m_Make_The_Bus_Off_Route()
        {
            IReadOnlyList<VehicleRouteState> v_States = Step([], At(49.903, 90));
            v_States = Step(v_States, At(49.9032, 60));
            Check.That(DeviationDetector.IsOffRoute(v_States.Single())).IsFalse();

            v_States = Step(v_States, At(49.9034, 30));

            Check.That(DeviationDetector.IsOffRoute(v_States.Single())).IsTrue();
            Check.That(v_States.Single().Track).HasSize(3);
        }

        [Fact]
        public void Same_Timestamp_Is_Not_Counted_Twice()
        {
            IReadOnlyList<VehicleRouteState> v_States = Step([], At(49.903, 30));
            v_States = Step(v_States, At(49.903, 30));
            v_States = Step(v_States, At(49.903, 30));

            Check.That(v_States.Single().OffRouteCount).IsEqualTo(1);
        }

        [Fact]
        public void Returning_On_Route_Resets_The_Counter_And_Track()
        {
            IReadOnlyList<VehicleRouteState> v_States = Step([], At(49.903, 90));
            v_States = Step(v_States, At(49.9032, 60));
            v_States = Step(v_States, At(49.9001, 30));

            Check.That(v_States.Single().OffRouteCount).IsEqualTo(0);
            Check.That(v_States.Single().Track).IsEmpty();
        }

        [Fact]
        public void Uncertain_Line_Or_Stale_Position_Is_Not_Tracked()
        {
            Check.That(Step([], At(49.903, 30), MatchConfidence.Low)).IsEmpty();
            Check.That(Step([], At(49.903, 600))).IsEmpty();
        }
    }
}
