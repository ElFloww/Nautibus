using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Core.UseCases.Network;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.Tests.UseCases.Network
{
    public class GetDeparturesUseCaseTests
    {
        private static readonly LineSummary m_Line = new("L1", "1", "Ligne 1", "E3C305", "17202A", 1);
        private readonly Mock<INetworkRepository> m_Network = new();
        private readonly Mock<IClock> m_Clock = new();

        public GetDeparturesUseCaseTests()
        {
            m_Network.Setup(p_N => p_N.GetStopAsync("AREA", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StopDetail(new StopSummary("AREA", "Gare", 0, 0, []), [new StopSummary("Q1", "Gare", 0, 0, [])]));
            m_Network.Setup(p_N => p_N.GetServiceCalendarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ServiceCalendarData(
                [new GtfsCalendar("SEM", 0b0011111, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30))], []));
            m_Network.Setup(p_N => p_N.GetStopTimesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
        }

        private Task<ValueResponse<IReadOnlyList<Departure>>> RunAsync(DateTime p_UtcNow, string p_Stop = "AREA")
        {
            m_Clock.Setup(p_C => p_C.UtcNow).Returns(p_UtcNow);
            return new GetDeparturesUseCase(Mock.Of<ILogger<GetDeparturesUseCase>>(), Mock.Of<IUnitOfWork>(), m_Network.Object, m_Clock.Object)
                .Handle(new GetDeparturesRequest(p_Stop), CancellationToken.None);
        }

        [Fact]
        public async Task Departures_Are_Converted_To_Paris_Time_On_Service_Day()
        {
            m_Network.Setup(p_N => p_N.GetStopTimesAsync(It.Is<IReadOnlyCollection<string>>(p_S => p_S.Contains("AREA") && p_S.Contains("Q1")),
                    It.Is<IReadOnlyCollection<string>>(p_S => p_S.Contains("SEM")), 39600, 39600 + 180 * 60, It.IsAny<CancellationToken>()))
                .ReturnsAsync([new ScheduledStopTime("T1", "SEM", "Q1", 40000, 0, "CHU", m_Line)]);

            // Jeudi 12/11/2026 10:00 UTC = 11:00 à Paris = 39600 s.
            ValueResponse<IReadOnlyList<Departure>> v_Result = await RunAsync(new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc));

            Check.That(v_Result.Value.Single().ScheduledTime).IsEqualTo(new DateTimeOffset(2026, 11, 12, 11, 6, 40, TimeSpan.FromHours(1)));
        }

        [Fact]
        public async Task After_Midnight_Trips_Of_Previous_Service_Day_Are_Included()
        {
            // Vendredi 13/11/2026 00:20 à Paris : la course de jeudi à 24:30:00 doit apparaître.
            m_Network.Setup(p_N => p_N.GetStopTimesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), 86400 + 1200, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new ScheduledStopTime("T9", "SEM", "Q1", 86400 + 1800, 0, "CHU", m_Line)]);
            m_Network.Setup(p_N => p_N.GetStopTimesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), 1200, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            ValueResponse<IReadOnlyList<Departure>> v_Result = await RunAsync(new DateTime(2026, 11, 12, 23, 20, 0, DateTimeKind.Utc));

            Check.That(v_Result.Value.Single().ScheduledTime).IsEqualTo(new DateTimeOffset(2026, 11, 13, 0, 30, 0, TimeSpan.FromHours(1)));
        }

        [Fact]
        public async Task Unknown_Stop_Returns_Not_Found()
        {
            ValueResponse<IReadOnlyList<Departure>> v_Result = await RunAsync(DateTime.UtcNow, "INCONNU");

            Check.That(v_Result.Errors.Single().Code).IsEqualTo(DomainErrors.Data.NotFound);
        }
    }
}
