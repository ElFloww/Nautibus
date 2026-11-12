using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Events;
using AmetisAPI.Domain.Interfaces.Push;
using AmetisAPI.Domain.Interfaces.Gtfs;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Realtime;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Data.EntityFramework.Repositories;
using AmetisAPI.Infrastructure.Gtfs;
using AmetisAPI.Infrastructure.Services;
using Autofac;

namespace AmetisAPI.Infrastructure
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            CustomLoad(builder);
        }


        /// <summary>
        /// Custom load for all custom DI
        /// </summary>
        /// <param name="p_Builder"></param>
        private static void CustomLoad(ContainerBuilder p_Builder)
        {
            // Add here your services
            p_Builder.RegisterType<UnitOfWork<AppDbContext>>()
                .As<IUnitOfWork>()
                .InstancePerLifetimeScope();

            p_Builder.RegisterType<FeedRepository>().As<IFeedRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<NetworkRepository>().As<INetworkRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<RealtimeReferenceRepository>().As<IRealtimeReferenceRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<VehicleHistoryRepository>().As<IVehicleHistoryRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<AccountRepository>().As<IAccountRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<FavoriteRepository>().As<IFavoriteRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<StopPassageRepository>().As<IStopPassageRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<NetworkEventRepository>().As<INetworkEventRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<PushSubscriptionRepository>().As<IPushSubscriptionRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<NetworkShapeRepository>().As<INetworkShapeRepository>().InstancePerLifetimeScope();
            p_Builder.RegisterType<GtfsZipParser>().As<IGtfsParser>().SingleInstance();
            p_Builder.RegisterType<GtfsRealtimeDecoder>().As<IGtfsRealtimeDecoder>().SingleInstance();
            p_Builder.RegisterType<SystemClock>().As<IClock>().SingleInstance();
        }
    }
}
