using AmetisAPI.Core.Realtime;

namespace AmetisAPI.Core.Tests.Realtime
{
    public class GeoMathTests
    {
        // Segment est-ouest à la latitude 49.9 entre les longitudes 2.30 et 2.31.
        private static readonly IReadOnlyList<double[]> m_Shape = [[2.30, 49.9], [2.31, 49.9]];

        [Fact]
        public void Point_On_Shape_Is_At_Zero_Meter()
        {
            Check.That(GeoMath.DistanceToShapesMeters(49.9, 2.305, [m_Shape])).IsCloseTo(0, 0.5);
        }

        [Fact]
        public void Point_North_Of_Segment_Is_At_Latitude_Delta()
        {
            // 0.001° de latitude ≈ 111,2 m
            Check.That(GeoMath.DistanceToShapesMeters(49.901, 2.305, [m_Shape])).IsCloseTo(111.2, 0.5);
        }

        [Fact]
        public void Point_Beyond_Segment_End_Is_Measured_To_The_End()
        {
            // 0.001° de longitude à 49.9° ≈ 71,6 m
            Check.That(GeoMath.DistanceToShapesMeters(49.9, 2.311, [m_Shape])).IsCloseTo(71.6, 0.5);
        }

        [Fact]
        public void Nearest_Of_Several_Shapes_Is_Used()
        {
            IReadOnlyList<double[]> v_Far = [[2.40, 49.95], [2.41, 49.95]];
            Check.That(GeoMath.DistanceToShapesMeters(49.901, 2.305, [v_Far, m_Shape])).IsCloseTo(111.2, 0.5);
        }
    }
}
