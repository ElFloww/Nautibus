namespace AmetisAPI.Core.Realtime
{
    /// <summary>Calculs de distance à l'échelle d'une agglomération (projection équirectangulaire locale, erreur négligeable sous 50 km).</summary>
    internal static class GeoMath
    {
        private const double EarthRadiusMeters = 6_371_000;

        /// <summary>Distance en mètres d'un point au tracé le plus proche (tracés en [longitude, latitude]).</summary>
        public static double DistanceToShapesMeters(double p_Latitude, double p_Longitude, IEnumerable<IReadOnlyList<double[]>> p_Shapes)
        {
            double v_CosLat = Math.Cos(p_Latitude * Math.PI / 180);
            (double X, double Y) Project(double p_Lon, double p_Lat) => (
                (p_Lon - p_Longitude) * Math.PI / 180 * EarthRadiusMeters * v_CosLat,
                (p_Lat - p_Latitude) * Math.PI / 180 * EarthRadiusMeters);

            double v_Best = double.PositiveInfinity;
            foreach (IReadOnlyList<double[]> v_Shape in p_Shapes)
            {
                for (int v_I = 0; v_I < v_Shape.Count; v_I++)
                {
                    (double v_Ax, double v_Ay) = Project(v_Shape[v_I][0], v_Shape[v_I][1]);
                    if (v_Shape.Count == 1)
                    {
                        v_Best = Math.Min(v_Best, Math.Sqrt(v_Ax * v_Ax + v_Ay * v_Ay));
                        break;
                    }
                    if (v_I == v_Shape.Count - 1)
                        break;
                    (double v_Bx, double v_By) = Project(v_Shape[v_I + 1][0], v_Shape[v_I + 1][1]);
                    v_Best = Math.Min(v_Best, DistanceToSegment(v_Ax, v_Ay, v_Bx, v_By));
                }
            }
            return v_Best;
        }

        /// <summary>Distance de l'origine (le véhicule) au segment AB.</summary>
        private static double DistanceToSegment(double p_Ax, double p_Ay, double p_Bx, double p_By)
        {
            double v_Dx = p_Bx - p_Ax, v_Dy = p_By - p_Ay;
            double v_Length = v_Dx * v_Dx + v_Dy * v_Dy;
            double v_T = v_Length == 0 ? 0 : Math.Clamp(-(p_Ax * v_Dx + p_Ay * v_Dy) / v_Length, 0, 1);
            double v_Px = p_Ax + v_T * v_Dx, v_Py = p_Ay + v_T * v_Dy;
            return Math.Sqrt(v_Px * v_Px + v_Py * v_Py);
        }
    }
}
