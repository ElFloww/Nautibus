using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Core.UseCases.Realtime;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Realtime;
using static AmetisAPI.Core.Tests.UseCases.Realtime.RealtimeTestData;

namespace AmetisAPI.Core.Tests.UseCases.Realtime
{
    public class GetStopBoardUseCaseTests
    {
        private readonly Mock<INetworkRepository> m_Network = new();
        private readonly Mock<IRealtimeReferenceRepository> m_References = new();
        private readonly Mock<IStopPassageRepository> m_Passages = new();
        private readonly Mock<IRealtimeStore> m_Store = new();
        private readonly Mock<IClock> m_Clock = new();
        private static readonly LineSummary m_Other = new("AMI:7-1", "7", "Ligne 7", "000000", "FFFFFF", 7);

        public GetStopBoardUseCaseTests()
        {
            m_Clock.Setup(p_C => p_C.UtcNow).Returns(m_Now); // 11:00 à Paris
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([]));
            m_Network.Setup(p_N => p_N.GetStopAsync("AREA", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StopDetail(new StopSummary("AREA", "Gare", 0, 0, []), [new StopSummary("Q1", "Gare", 0, 0, [])]));
            m_Network.Setup(p_N => p_N.GetServiceCalendarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ServiceCalendarData(
                [new GtfsCalendar("SEM", 0b0011111, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30))], []));
            m_Network.Setup(p_N => p_N.GetStopTimesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            // Jeudi 12/11 : départs théoriques 11:05 (5A) et 11:20 (7), en secondes du jour de service.
            m_Network.Setup(p_N => p_N.GetStopTimesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), 38400, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new ScheduledStopTime("N1", "SEM", "Q1", 39900, 0, "CHU", m_Line), new ScheduledStopTime("N2", "SEM", "Q1", 40800, 0, "Gare", m_Other)]);
            m_References.Setup(p_R => p_R.GetStopMatchesForNetworkStopsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new RealtimeStopMatch("905", "Q1", 3, MatchConfidence.High)]);
            m_References.Setup(p_R => p_R.GetLineMatchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new RealtimeLineMatch("5A", m_Line, MatchConfidence.High)]);
            m_References.Setup(p_R => p_R.GetTripsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new RealtimeTripInfo("R1", "5A", "CHU A. PICARDIE", 0), new RealtimeTripInfo("R0", "5A", "CHU A. PICARDIE", 0)]);
        }

        private Task<ValueResponse<StopBoard>> RunAsync() =>
            new GetStopBoardUseCase(Mock.Of<ILogger<GetStopBoardUseCase>>(), Mock.Of<IUnitOfWork>(), m_Network.Object, m_References.Object, m_Passages.Object, m_Store.Object, m_Clock.Object)
                .Handle(new GetStopBoardRequest("AREA"), CancellationToken.None);

        [Fact]
        public void Realtime_Passage_Completes_The_Matching_Scheduled_Departure_And_Past_Ones_Go_To_Recent()
        {
            m_Passages.Setup(p_P => p_P.ListForStopsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([
                    // Prévu 10:05:40 UTC (11:05:40 Paris, à 40 s du théorique), effectif 11:08:40 : 3 min de retard.
                    new StopPassage("R1", "5A", "905", 4, m_Now.AddMinutes(5).AddSeconds(40), m_Now.AddMinutes(8).AddSeconds(40), 180, "718"),
                    // Passé il y a 4 min avec 1 min d'avance.
                    new StopPassage("R0", "5A", "905", 4, m_Now.AddMinutes(-3), m_Now.AddMinutes(-4), -60, "719"),
                    // Terminus : arrivée sans montée, pas dans les prochains départs.
                    new StopPassage("R2", "5A", "905", 9, m_Now.AddMinutes(30), m_Now.AddMinutes(30), 0, "720", true)
                ]);

            StopBoard v_Board = RunAsync().Result.Value;

            Check.That(v_Board.Upcoming).HasSize(2);
            StopBoardEntry v_First = v_Board.Upcoming[0];
            Check.That(v_First.IsRealtime).IsTrue();
            Check.That(v_First.ScheduledTime).IsEqualTo(new DateTimeOffset(2026, 11, 12, 11, 5, 0, TimeSpan.FromHours(1)));
            Check.That(v_First.ExpectedTime).IsEqualTo(new DateTimeOffset(2026, 11, 12, 11, 8, 40, TimeSpan.FromHours(1)));
            Check.That(v_First.DelaySeconds).IsEqualTo(180);
            Check.That(v_First.TripId).IsEqualTo("R1");
            Check.That(v_First.Destination).IsEqualTo("CHU");
            Check.That(v_Board.Upcoming[1].IsRealtime).IsFalse();
            Check.That(v_Board.Upcoming[1].ExpectedTime).IsNull();

            StopBoardEntry v_Recent = v_Board.Recent.Single();
            Check.That(v_Recent.DelaySeconds).IsEqualTo(-60);
            Check.That(v_Recent.ExpectedTime).IsEqualTo(new DateTimeOffset(2026, 11, 12, 10, 56, 0, TimeSpan.FromHours(1)));
        }

        [Fact]
        public void Bus_Just_Passed_Removes_Its_Scheduled_Departure_From_Upcoming()
        {
            // Départ théorique 11:00 (39600 s) ; le bus est passé à 10:59:30 (30 s d'avance).
            m_Network.Setup(p_N => p_N.GetStopTimesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), 38400, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new ScheduledStopTime("N1", "SEM", "Q1", 39600, 0, "CHU", m_Line)]);
            m_Passages.Setup(p_P => p_P.ListForStopsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new StopPassage("R0", "5A", "905", 4, m_Now, m_Now.AddSeconds(-30), -30, "719")]);

            StopBoard v_Board = RunAsync().Result.Value;

            Check.That(v_Board.Upcoming).IsEmpty();
            Check.That(v_Board.Recent.Single().DelaySeconds).IsEqualTo(-30);
        }
    }
}
