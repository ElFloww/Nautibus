using AmetisAPI.Core.Requests.Events;
using AmetisAPI.Core.UseCases.Events;
using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Push;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Events;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Push;
using AmetisAPI.Domain.Interfaces.Realtime;
using static AmetisAPI.Core.Tests.UseCases.Realtime.RealtimeTestData;

namespace AmetisAPI.Core.Tests.UseCases.Events
{
    public class ProcessNetworkEventsUseCaseTests
    {
        private readonly Mock<IRealtimeStore> m_Store = new();
        private readonly Mock<IRealtimeReferenceRepository> m_References = new();
        private readonly Mock<INetworkShapeRepository> m_Shapes = new();
        private readonly Mock<INetworkRepository> m_Network = new();
        private readonly Mock<INetworkEventRepository> m_Events = new();
        private readonly Mock<IPushSubscriptionRepository> m_Subscriptions = new();
        private readonly Mock<IPushSender> m_Push = new();
        private readonly Mock<IClock> m_Clock = new();
        private RealtimeSnapshot m_Saved;
        private IReadOnlyList<NetworkEventDraft> m_DetectedDrafts;

        private static readonly PushSubscriptionInfo m_Alive = new("https://fcm.googleapis.com/a", "k", "a");
        private static readonly PushSubscriptionInfo m_Expired = new("https://fcm.googleapis.com/b", "k", "a");

        public ProcessNetworkEventsUseCaseTests()
        {
            m_Clock.Setup(p_C => p_C.UtcNow).Returns(m_Now);
            m_References.Setup(p_R => p_R.GetLineMatchesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new RealtimeLineMatch("5A", m_Line, MatchConfidence.High)]);
            m_References.Setup(p_R => p_R.GetTripsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new RealtimeTripInfo("2-5A-1", "5A", "CHU", 0)]);
            // Tracé loin des positions de test (49.9, 2.3) : tout bus est hors tracé.
            m_Shapes.Setup(p_S => p_S.GetShapesByLineAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, IReadOnlyList<IReadOnlyList<double[]>>> { [m_Line.Id] = [[[2.40, 49.95], [2.41, 49.95]]] });
            m_Network.Setup(p_N => p_N.SearchLinesAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync([m_Line]);
            m_Store.Setup(p_S => p_S.SaveAsync(It.IsAny<RealtimeSnapshot>(), It.IsAny<CancellationToken>()))
                .Callback<RealtimeSnapshot, CancellationToken>((p_S, _) => m_Saved = p_S).Returns(Task.CompletedTask);
            m_Events.Setup(p_E => p_E.SyncAsync(NetworkEventKind.Official, It.IsAny<IReadOnlyList<NetworkEventDraft>>(), m_Now, TimeSpan.Zero, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            m_Events.Setup(p_E => p_E.SyncAsync(NetworkEventKind.DetectedDetour, It.IsAny<IReadOnlyList<NetworkEventDraft>>(), m_Now, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Callback<NetworkEventKind, IReadOnlyList<NetworkEventDraft>, DateTime, TimeSpan, CancellationToken>((_, p_D, _, _, _) => m_DetectedDrafts = p_D)
                .ReturnsAsync(() => m_DetectedDrafts.Select(p_D => new NetworkEvent(9, NetworkEventKind.DetectedDetour, p_D.ExternalId, p_D.Title, p_D.Description, null, p_D.LineIds, m_Now, null, null, m_Now, p_D.Confidence)).ToList());
            m_Push.Setup(p_P => p_P.IsEnabled).Returns(true);
            m_Subscriptions.Setup(p_S => p_S.ListForLinesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([m_Alive, m_Expired]);
            m_Push.Setup(p_P => p_P.SendAsync(m_Alive, It.IsAny<PushMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(PushResult.Sent);
            m_Push.Setup(p_P => p_P.SendAsync(m_Expired, It.IsAny<PushMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(PushResult.Gone);
        }

        private Task<ValueResponse<int>> RunAsync() =>
            new ProcessNetworkEventsUseCase(Mock.Of<ILogger<ProcessNetworkEventsUseCase>>(), Mock.Of<IUnitOfWork>(), m_Store.Object, m_References.Object, m_Shapes.Object,
                m_Network.Object, m_Events.Object, m_Subscriptions.Object, m_Push.Object, m_Clock.Object).Handle(new ProcessNetworkEventsRequest(), CancellationToken.None);

        [Fact]
        public async Task Sustained_Off_Route_Bus_Creates_Detour_Event_And_Notifies_Subscribers()
        {
            // Deux positions hors tracé déjà comptées, une nouvelle position : le seuil de 3 est atteint.
            VehicleRouteState v_Previous = new("A", m_Line.Id, 2, m_Now.AddSeconds(-60), 6000, [[2.3, 49.9], [2.3, 49.9]]);
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([Vehicle("A", 30)]) with { RouteStates = [v_Previous] });

            ValueResponse<int> v_Result = await RunAsync();

            Check.That(v_Result.Value).IsEqualTo(1);
            Check.That(m_Saved.RouteStates.Single().OffRouteCount).IsEqualTo(3);
            Check.That(m_DetectedDrafts.Single().Title).IsEqualTo("Déviation probable – ligne 5A");
            m_Push.Verify(p_P => p_P.SendAsync(m_Alive, It.Is<PushMessage>(p_M => p_M.Url == "/infos-trafic"), It.IsAny<CancellationToken>()), Times.Once);
            m_Subscriptions.Verify(p_S => p_S.RemoveByEndpointAsync(m_Expired.Endpoint, It.IsAny<CancellationToken>()), Times.Once);
            m_Subscriptions.Verify(p_S => p_S.RemoveByEndpointAsync(m_Alive.Endpoint, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task No_Notification_When_Push_Is_Not_Configured()
        {
            m_Push.Setup(p_P => p_P.IsEnabled).Returns(false);
            VehicleRouteState v_Previous = new("A", m_Line.Id, 2, m_Now.AddSeconds(-60), 6000, []);
            m_Store.Setup(p_S => p_S.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Snapshot([Vehicle("A", 30)]) with { RouteStates = [v_Previous] });

            await RunAsync();

            m_Push.Verify(p_P => p_P.SendAsync(It.IsAny<PushSubscriptionInfo>(), It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
