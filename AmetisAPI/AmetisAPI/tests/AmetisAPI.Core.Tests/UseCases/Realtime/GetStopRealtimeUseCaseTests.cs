using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Core.UseCases.Realtime;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Realtime;
using static AmetisAPI.Core.Tests.UseCases.Realtime.RealtimeTestData;

namespace AmetisAPI.Core.Tests.UseCases.Realtime
{
    public class GetStopRealtimeUseCaseTests
    {
        private readonly Mock<IRealtimeStore> m_Store = new();
        private readonly Mock<INetworkRepository> m_Network = new();
        private readonly Mock<IRealtimeReferenceRepository> m_References = new();
        private readonly Mock<IClock> m_Clock = new();

        public GetStopRealtimeUseCaseTests()
        {
            m_Clock.Setup(p_C => p_C.UtcNow).Returns(m_Now);
            m_Network.Setup(p_N => p_N.GetStopAsync("AREA", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StopDetail(new StopSummary("AREA", "Gare", 0, 0, []), [new StopSummary("Q1", "Gare", 0, 0, [])]));
            m_References.Setup(p_R => p_R.GetStopMatchesForNetworkStopsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new RealtimeStopMatch("905", "Q1", 4, MatchConfidence.Medium)]);
            m_References.Setup(p_R => p_R.GetLineMatchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new RealtimeLineMatch("5A", m_Line, MatchConfidence.High)]);
            m_References.Setup(p_R => p_R.GetTripsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new RealtimeTripInfo("2-5A-1", "5A", "CHU A. PICARDIE", 0)]);
        }

        [Fact]
        public async Task Arrivals_Use_Weakest_Link_Confidence_And_Ignore_Past_And_Other_Stops()
        {
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([], [
                new StopTimePrediction("2-5A-1", "5A", "905", 3, null, m_Now.AddMinutes(4), 60, "718"),
                new StopTimePrediction("2-5A-1", "5A", "905", 3, null, m_Now.AddMinutes(-5), 0, "718"),
                new StopTimePrediction("2-5A-1", "5A", "AUTRE", 4, null, m_Now.AddMinutes(6), 0, "718")
            ]));

            IReadOnlyList<RealtimeArrival> v_Arrivals = (await new GetStopRealtimeUseCase(Mock.Of<ILogger<GetStopRealtimeUseCase>>(), Mock.Of<IUnitOfWork>(),
                m_Store.Object, m_Network.Object, m_References.Object, m_Clock.Object).Handle(new GetStopRealtimeRequest("AREA"), CancellationToken.None)).Value;

            RealtimeArrival v_Arrival = v_Arrivals.Single();
            Check.That(v_Arrival.StopId).IsEqualTo("Q1");
            Check.That(v_Arrival.Confidence).IsEqualTo(MatchConfidence.Medium);
            Check.That(v_Arrival.Destination).IsEqualTo("CHU A. PICARDIE");
            Check.That(v_Arrival.ExpectedTime).IsEqualTo(new DateTimeOffset(2026, 11, 12, 11, 4, 0, TimeSpan.FromHours(1)));
            Check.That(v_Arrival.Provenance).IsEqualTo("ametis-trip-updates");
            Check.That(v_Arrival.IsStale).IsFalse();
        }
    }
}
