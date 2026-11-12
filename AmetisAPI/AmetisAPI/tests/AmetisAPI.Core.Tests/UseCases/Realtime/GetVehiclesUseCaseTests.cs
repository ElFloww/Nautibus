using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Core.UseCases.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Realtime;
using static AmetisAPI.Core.Tests.UseCases.Realtime.RealtimeTestData;

namespace AmetisAPI.Core.Tests.UseCases.Realtime
{
    public class GetVehiclesUseCaseTests
    {
        private readonly Mock<IRealtimeStore> m_Store = new();
        private readonly Mock<IRealtimeReferenceRepository> m_References = new();
        private readonly Mock<IClock> m_Clock = new();

        public GetVehiclesUseCaseTests()
        {
            m_Clock.Setup(p_C => p_C.UtcNow).Returns(m_Now);
            m_References.Setup(p_R => p_R.GetLineMatchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new RealtimeLineMatch("5A", m_Line, MatchConfidence.High)]);
            m_References.Setup(p_R => p_R.GetTripsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new RealtimeTripInfo("2-5A-1", "5A", "CHU A. PICARDIE", 0)]);
        }

        private Task<ValueResponse<IReadOnlyList<VehicleView>>> RunAsync() =>
            new GetVehiclesUseCase(Mock.Of<ILogger<GetVehiclesUseCase>>(), Mock.Of<IUnitOfWork>(), m_Store.Object, m_References.Object, m_Clock.Object)
                .Handle(new GetVehiclesRequest(), CancellationToken.None);

        [Fact]
        public async Task Vehicles_Are_Linked_To_Reference_Line_With_Freshness()
        {
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([Vehicle("A", 30), Vehicle("B", 300), Vehicle("C", 900)]));

            IReadOnlyList<VehicleView> v_Vehicles = (await RunAsync()).Value;

            Check.That(v_Vehicles.Select(p_V => p_V.VehicleId)).ContainsExactly("A", "B");
            Check.That(v_Vehicles[0].IsStale).IsFalse();
            Check.That(v_Vehicles[1].IsStale).IsTrue();
            Check.That(v_Vehicles[0].Line).IsEqualTo(m_Line);
            Check.That(v_Vehicles[0].Destination).IsEqualTo("CHU A. PICARDIE");
            Check.That(v_Vehicles[0].SpeedKmh).IsEqualTo(36);
            Check.That(v_Vehicles[0].LineConfidence).IsEqualTo(MatchConfidence.High);
        }

        [Fact]
        public async Task Vehicle_With_Unknown_Trip_Has_Low_Confidence()
        {
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([Vehicle("A", 30, "COURSE_INCONNUE")]));

            Check.That((await RunAsync()).Value.Single().LineConfidence).IsEqualTo(MatchConfidence.Low);
        }

        [Fact]
        public async Task No_Collect_Yet_Returns_Empty_List()
        {
            Check.That((await RunAsync()).Value).IsEmpty();
        }

        [Fact]
        public async Task Next_Stop_Is_The_First_Not_Yet_Passed_In_Feed_Order()
        {
            m_References.Setup(p_R => p_R.GetRealtimeStopNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, string> { ["A"] = "Alsace Lorraine", ["B"] = "Place du Don" });
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([Vehicle("A", 30)], [
                new StopTimePrediction("2-5A-1", "5A", "A", null, null, m_Now.AddSeconds(-30), 300, "A"),
                new StopTimePrediction("2-5A-1", "5A", "B", null, null, m_Now.AddMinutes(2), 280, "A")
            ]));

            VehicleView v_Vehicle = (await RunAsync()).Value.Single();

            Check.That(v_Vehicle.NextStopName).IsEqualTo("Place du Don");
            Check.That(v_Vehicle.DelaySeconds).IsEqualTo(280);
        }
    }
}
