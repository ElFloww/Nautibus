using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Core.UseCases.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Realtime;
using static AmetisAPI.Core.Tests.UseCases.Realtime.RealtimeTestData;

namespace AmetisAPI.Core.Tests.UseCases.Realtime
{
    public class GetTripUseCaseTests
    {
        private readonly Mock<IStopPassageRepository> m_Passages = new();
        private readonly Mock<IRealtimeReferenceRepository> m_References = new();
        private readonly Mock<IVehicleHistoryRepository> m_History = new();
        private readonly Mock<IRealtimeStore> m_Store = new();
        private readonly Mock<IClock> m_Clock = new();

        public GetTripUseCaseTests()
        {
            m_Clock.Setup(p_C => p_C.UtcNow).Returns(m_Now);
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([Vehicle("718", 20)]));
            m_References.Setup(p_R => p_R.GetRealtimeStopsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new RealtimeStopInfo("A", "Gare", 49.89, 2.30), new RealtimeStopInfo("B", "Cathédrale", 49.894, 2.302), new RealtimeStopInfo("C", "CHU", 49.87, 2.25)]);
            m_References.Setup(p_R => p_R.GetNetworkStopsForRealtimeStopsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, string> { ["A"] = "Q1" });
            m_References.Setup(p_R => p_R.GetTripsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([new RealtimeTripInfo("2-5A-1", "5A", "CHU", 0)]);
            m_References.Setup(p_R => p_R.GetLineMatchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new RealtimeLineMatch("5A", m_Line, MatchConfidence.High)]);
            m_History.Setup(p_H => p_H.GetTripTraceAsync("2-5A-1", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([[2.30, 49.90], [2.31, 49.90]]);
        }

        private Task<ValueResponse<TripDetail>> RunAsync() =>
            new GetTripUseCase(Mock.Of<ILogger<GetTripUseCase>>(), Mock.Of<IUnitOfWork>(), m_Passages.Object, m_References.Object, m_History.Object, m_Store.Object, m_Clock.Object)
                .Handle(new GetTripRequest("2-5A-1"), CancellationToken.None);

        [Fact]
        public async Task Stops_Are_Passed_Next_Or_Upcoming_With_Scheduled_And_Effective_Times()
        {
            m_Passages.Setup(p_P => p_P.ListForTripAsync("2-5A-1", m_Now, It.IsAny<CancellationToken>())).ReturnsAsync([
                new StopPassage("2-5A-1", "5A", "A", 1, m_Now.AddMinutes(-10), m_Now.AddMinutes(-8), 120, "718"),
                new StopPassage("2-5A-1", "5A", "B", 2, m_Now.AddMinutes(-1), m_Now.AddMinutes(1), 120, "718"),
                new StopPassage("2-5A-1", "5A", "C", 3, m_Now.AddMinutes(5), m_Now.AddMinutes(6), 60, "718")
            ]);

            TripDetail v_Trip = (await RunAsync()).Value;

            Check.That(v_Trip.Stops.Select(p_S => p_S.Status)).ContainsExactly(TripStopStatus.Passed, TripStopStatus.Next, TripStopStatus.Upcoming);
            Check.That(v_Trip.Stops[0].StopName).IsEqualTo("Gare");
            Check.That(v_Trip.Stops[0].NetworkStopId).IsEqualTo("Q1");
            Check.That(v_Trip.Stops[2].Latitude).IsEqualTo(49.87);
            Check.That(v_Trip.Stops[2].Longitude).IsEqualTo(2.25);
            Check.That(v_Trip.Stops[0].ScheduledTime).IsEqualTo(new DateTimeOffset(2026, 11, 12, 10, 50, 0, TimeSpan.FromHours(1)));
            Check.That(v_Trip.Stops[0].ExpectedTime).IsEqualTo(new DateTimeOffset(2026, 11, 12, 10, 52, 0, TimeSpan.FromHours(1)));
            Check.That(v_Trip.Line).IsEqualTo(m_Line);
            Check.That(v_Trip.VehicleId).IsEqualTo("718");
            Check.That(v_Trip.Trace).HasSize(2);
        }

        [Fact]
        public async Task Unknown_Trip_Is_Not_Found()
        {
            m_Passages.Setup(p_P => p_P.ListForTripAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

            Check.That((await RunAsync()).Errors.Single().Code).IsEqualTo(DomainErrors.Data.NotFound);
        }
    }
}
