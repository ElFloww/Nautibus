using AmetisAPI.Core.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Tests.Gtfs
{
    public class DataHealthEvaluatorTests
    {
        private static readonly DateOnly m_Today = new(2026, 11, 12);
        private static readonly RealtimeStatus m_FreshRealtime = new(DateTime.UtcNow, DateTime.UtcNow, 30, false, 50, 0, []);

        private static FeedVersionInfo Version(int p_Id, FeedSource p_Source, FeedImportStatus p_Status, bool p_Active, DateOnly? p_ValidTo = null, string p_Error = null)
            => new(p_Id, p_Source, p_Status, p_Active, "H", DateTime.UtcNow, DateTime.UtcNow, 1, 1, 1, 1, 0, null, p_ValidTo ?? new DateOnly(2027, 1, 1), null, p_Error);

        [Fact]
        public void Both_Feeds_Active_And_Fresh_Realtime_Is_Healthy()
        {
            DataHealth v_Health = DataHealthEvaluator.Evaluate(
                [Version(1, FeedSource.Network, FeedImportStatus.Succeeded, true), Version(2, FeedSource.Realtime, FeedImportStatus.Succeeded, true)],
                m_FreshRealtime, m_Today);

            Check.That(v_Health.Status).IsEqualTo(DataHealthStatus.Healthy);
        }

        [Fact]
        public void No_Active_Network_Is_Unhealthy()
        {
            DataHealth v_Health = DataHealthEvaluator.Evaluate([Version(2, FeedSource.Realtime, FeedImportStatus.Succeeded, true)], m_FreshRealtime, m_Today);

            Check.That(v_Health.Status).IsEqualTo(DataHealthStatus.Unhealthy);
        }

        [Fact]
        public void Failed_Latest_Import_Or_Expired_Data_Or_Stale_Realtime_Is_Degraded()
        {
            DataHealth v_Failed = DataHealthEvaluator.Evaluate(
                [Version(1, FeedSource.Network, FeedImportStatus.Succeeded, true), Version(3, FeedSource.Network, FeedImportStatus.Failed, false, p_Error: "zip"),
                 Version(2, FeedSource.Realtime, FeedImportStatus.Succeeded, true)],
                m_FreshRealtime, m_Today);
            DataHealth v_Expired = DataHealthEvaluator.Evaluate(
                [Version(1, FeedSource.Network, FeedImportStatus.Succeeded, true), Version(2, FeedSource.Realtime, FeedImportStatus.Succeeded, true, new DateOnly(2026, 11, 11))],
                m_FreshRealtime, m_Today);
            DataHealth v_Stale = DataHealthEvaluator.Evaluate(
                [Version(1, FeedSource.Network, FeedImportStatus.Succeeded, true), Version(2, FeedSource.Realtime, FeedImportStatus.Succeeded, true)],
                m_FreshRealtime with { IsStale = true, FeedAgeSeconds = 900 }, m_Today);

            Check.That(v_Failed.Status).IsEqualTo(DataHealthStatus.Degraded);
            Check.That(v_Failed.Messages.Single()).Contains("dernier import en échec (zip)");
            Check.That(v_Expired.Status).IsEqualTo(DataHealthStatus.Degraded);
            Check.That(v_Stale.Messages.Single()).IsEqualTo("Temps réel obsolète (900 s)");
        }
    }
}
