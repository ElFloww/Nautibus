using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Gtfs
{
    /// <summary>
    /// Règles de supervision : pas de réseau actif = hors service ; import récent en échec, données hors période
    /// de validité ou temps réel obsolète = dégradé.
    /// </summary>
    internal static class DataHealthEvaluator
    {
        public static DataHealth Evaluate(IReadOnlyList<FeedVersionInfo> p_RecentVersions, RealtimeStatus p_Realtime, DateOnly p_Today)
        {
            List<string> v_Messages = [];
            DataHealthStatus v_Status = DataHealthStatus.Healthy;

            foreach (FeedSource v_Source in Enum.GetValues<FeedSource>())
            {
                List<FeedVersionInfo> v_Versions = p_RecentVersions.Where(p_V => p_V.Source == v_Source).OrderByDescending(p_V => p_V.Id).ToList();
                FeedVersionInfo v_Active = v_Versions.FirstOrDefault(p_V => p_V.IsActive);
                FeedVersionInfo v_Latest = v_Versions.FirstOrDefault(p_V => p_V.Status != FeedImportStatus.Running);

                if (v_Active is null)
                {
                    v_Messages.Add($"{v_Source} : aucune version active");
                    v_Status = v_Source == FeedSource.Network ? DataHealthStatus.Unhealthy : Max(v_Status, DataHealthStatus.Degraded);
                    continue;
                }
                if (v_Latest?.Status == FeedImportStatus.Failed)
                {
                    v_Messages.Add($"{v_Source} : dernier import en échec ({v_Latest.ErrorMessage})");
                    v_Status = Max(v_Status, DataHealthStatus.Degraded);
                }
                if (v_Active.ValidTo is { } v_End && v_End < p_Today)
                {
                    v_Messages.Add($"{v_Source} : données expirées depuis le {v_End:yyyy-MM-dd}");
                    v_Status = Max(v_Status, DataHealthStatus.Degraded);
                }
            }

            if (p_Realtime.IsStale)
            {
                v_Messages.Add(p_Realtime.FeedAgeSeconds is { } v_Age ? $"Temps réel obsolète ({v_Age} s)" : "Temps réel indisponible");
                v_Status = Max(v_Status, DataHealthStatus.Degraded);
            }
            v_Messages.AddRange(p_Realtime.Errors);
            return new DataHealth(v_Status, v_Messages);
        }

        private static DataHealthStatus Max(DataHealthStatus p_A, DataHealthStatus p_B) => p_A > p_B ? p_A : p_B;
    }
}
