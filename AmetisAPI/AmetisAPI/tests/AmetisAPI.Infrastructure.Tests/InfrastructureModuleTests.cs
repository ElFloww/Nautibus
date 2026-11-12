using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Events;
using AmetisAPI.Domain.Interfaces.Push;
using AmetisAPI.Domain.Interfaces.Gtfs;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Infrastructure.Tests
{
    public class InfrastructureModuleTests
    {
        private static readonly string m_AutofacClass = $"{nameof(Autofac)}.";

        private readonly IList<Type> m_TypesWanted =
        [
            typeof(IUnitOfWork),
            typeof(IFeedRepository),
            typeof(INetworkRepository),
            typeof(IRealtimeReferenceRepository),
            typeof(IVehicleHistoryRepository),
            typeof(IAccountRepository),
            typeof(IFavoriteRepository),
            typeof(IGtfsParser),
            typeof(IGtfsRealtimeDecoder),
            typeof(IClock),
            typeof(INetworkEventRepository),
            typeof(IPushSubscriptionRepository),
            typeof(INetworkShapeRepository),
            typeof(IStopPassageRepository),
        ];

        private IList<Type> m_TypesLoaded;

        [Fact]
        public void Should_Have_Register_Types()
        {
            ContainerBuilder v_ContainerBuilder = new();
            v_ContainerBuilder.RegisterModule(new InfrastructureModule());

            IContainer v_Container = v_ContainerBuilder.Build();

            m_TypesLoaded = v_Container.ComponentRegistry.Registrations
                .SelectMany(p_X => p_X.Services)
                .Cast<IServiceWithType>()
                .Select(p_X => p_X.ServiceType)
                // Exclude types registered by AutoFac
                .Where(p_C => !p_C.FullName.StartsWith(m_AutofacClass))
                .Distinct()
                .ToList();

            Check.That(m_TypesLoaded.Where(p_C => p_C.Name != nameof(Castle.Core.Configuration.IConfiguration)).OrderBy(p_C => p_C.FullName)).ContainsExactly(m_TypesWanted.OrderBy(p_C => p_C.FullName));
        }
    }
}