using AmetisAPI.Core.Gtfs;

namespace AmetisAPI.Core.Tests.Gtfs
{
    public class FeedChangeSummaryTests
    {
        [Fact]
        public void Summary_Counts_And_Lists_Removed_Identifiers()
        {
            string v_Summary = FeedChangeSummary.Build(["L1", "L2"], ["L2", "L3", "L4"], ["A"], ["A"]);

            Check.That(v_Summary).IsEqualTo("Lignes : +2 -1 (supprimés : L1) | Arrêts : +0 -0");
        }
    }
}
