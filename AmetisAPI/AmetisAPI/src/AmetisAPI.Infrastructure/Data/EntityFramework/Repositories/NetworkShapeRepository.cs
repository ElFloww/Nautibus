using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Interfaces.Network;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Tracés par ligne, gardés en mémoire tant que la version active du réseau ne change pas
    /// (≈ 52 000 points relus sinon à chaque collecte de 30 s).
    /// </summary>
    internal class NetworkShapeRepository(AppDbContext p_Context) : INetworkShapeRepository
    {
        private static (int Version, IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>> Shapes) m_Cache;

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>>> GetShapesByLineAsync(CancellationToken p_CancellationToken)
        {
            int v_Version = await p_Context.FeedVersions.Where(p_V => p_V.Source == (byte)FeedSource.Network && p_V.IsActive)
                .Select(p_V => p_V.Id).SingleOrDefaultAsync(p_CancellationToken);
            (int, IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>>) v_Cached = m_Cache;
            if (v_Cached.Item1 == v_Version && v_Cached.Item2 is not null)
                return v_Cached.Item2;

            var v_LineShapes = await p_Context.Trips.Where(p_T => p_T.FeedVersionId == v_Version && p_T.ShapeId != null)
                .Select(p_T => new { p_T.RouteId, p_T.ShapeId }).Distinct().ToListAsync(p_CancellationToken);
            var v_Points = await p_Context.ShapePoints.AsNoTracking().Where(p_P => p_P.FeedVersionId == v_Version)
                .OrderBy(p_P => p_P.ShapeId).ThenBy(p_P => p_P.Sequence)
                .Select(p_P => new { p_P.ShapeId, p_P.Longitude, p_P.Latitude }).ToListAsync(p_CancellationToken);
            Dictionary<string, IReadOnlyList<double[]>> v_ByShape = v_Points.GroupBy(p_P => p_P.ShapeId)
                .ToDictionary(p_G => p_G.Key, p_G => (IReadOnlyList<double[]>)p_G.Select(p_P => new[] { p_P.Longitude, p_P.Latitude }).ToList());

            IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>> v_Result = v_LineShapes.GroupBy(p_L => p_L.RouteId)
                .ToDictionary(p_G => p_G.Key, p_G => (IReadOnlyList<IReadOnlyList<double[]>>)p_G
                    .Select(p_L => v_ByShape.GetValueOrDefault(p_L.ShapeId)).Where(p_S => p_S is not null).ToList());
            m_Cache = (v_Version, v_Result);
            return v_Result;
        }
    }
}
