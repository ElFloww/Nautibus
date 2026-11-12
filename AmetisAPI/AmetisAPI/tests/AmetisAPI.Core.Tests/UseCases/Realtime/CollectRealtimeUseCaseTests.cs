using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Core.UseCases.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Realtime;
using static AmetisAPI.Core.Tests.UseCases.Realtime.RealtimeTestData;

namespace AmetisAPI.Core.Tests.UseCases.Realtime
{
    public class CollectRealtimeUseCaseTests
    {
        private readonly Mock<IGtfsRealtimeSource> m_Source = new();
        private readonly Mock<IGtfsRealtimeDecoder> m_Decoder = new();
        private readonly Mock<IRealtimeStore> m_Store = new();
        private readonly Mock<IVehicleHistoryRepository> m_History = new();
        private readonly Mock<IStopPassageRepository> m_Passages = new();
        private readonly Mock<IClock> m_Clock = new();
        private RealtimeSnapshot m_Saved;

        public CollectRealtimeUseCaseTests()
        {
            m_Clock.Setup(p_C => p_C.UtcNow).Returns(m_Now);
            m_Store.Setup(p_S => p_S.SaveAsync(It.IsAny<RealtimeSnapshot>(), It.IsAny<CancellationToken>()))
                .Callback<RealtimeSnapshot, CancellationToken>((p_S, _) => m_Saved = p_S).Returns(Task.CompletedTask);
            m_Source.Setup(p_S => p_S.FetchTripUpdatesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([2]);
            m_Source.Setup(p_S => p_S.FetchAlertsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([3]);
            m_Decoder.Setup(p_D => p_D.Decode(It.Is<byte[]>(p_B => p_B[0] == 2))).Returns(new RealtimeFeed(m_Now, [], [], [], ["Course X : heures non croissantes"]));
            m_Decoder.Setup(p_D => p_D.Decode(It.Is<byte[]>(p_B => p_B[0] == 3))).Returns(new RealtimeFeed(m_Now, [], [], [], []));
        }

        private Task<ValueResponse<RealtimeStatus>> RunAsync() =>
            new CollectRealtimeUseCase(Mock.Of<ILogger<CollectRealtimeUseCase>>(), Mock.Of<IUnitOfWork>(), m_Source.Object, m_Decoder.Object,
                m_Store.Object, m_History.Object, m_Passages.Object, m_Clock.Object).Handle(new CollectRealtimeRequest(), CancellationToken.None);

        [Fact]
        public async Task Unavailable_Feed_Keeps_Previous_Vehicles_And_Reports_Error()
        {
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([Vehicle("A", 60)]));
            m_Source.Setup(p_S => p_S.FetchVehiclePositionsAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("timeout"));

            ValueResponse<RealtimeStatus> v_Result = await RunAsync();

            Check.That(m_Saved.Vehicles.Single().VehicleId).IsEqualTo("A");
            Check.That(m_Saved.Errors.Single()).Contains("vehicle-positions");
            Check.That(m_Saved.Anomalies.Single()).Contains("heures non croissantes");
            Check.That(v_Result.Value.Errors).HasSize(1);
            m_History.Verify(p_H => p_H.AppendAsync(It.IsAny<IReadOnlyCollection<VehiclePosition>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Fresh_Vehicles_Are_Stored_And_Appended_To_History()
        {
            m_Source.Setup(p_S => p_S.FetchVehiclePositionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([1]);
            m_Decoder.Setup(p_D => p_D.Decode(It.Is<byte[]>(p_B => p_B[0] == 1))).Returns(new RealtimeFeed(m_Now, [Vehicle("B", 5)], [], [], []));

            await RunAsync();

            Check.That(m_Saved.Vehicles.Single().VehicleId).IsEqualTo("B");
            Check.That(m_Saved.Errors).IsEmpty();
            m_History.Verify(p_H => p_H.AppendAsync(It.Is<IReadOnlyCollection<VehiclePosition>>(p_V => p_V.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
