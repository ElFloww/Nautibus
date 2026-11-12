using System.Configuration;
using AmetisAPI.Core;
using AmetisAPI.Core.Hosting;
using AmetisAPI.Infrastructure;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Cortex.Mediator.DependencyInjection;
using Serilog;

namespace AmetisAPI.Worker
{
    /// <summary>
    /// Processus de fond : imports GTFS (démarrage puis toutes les 6 h), collecte GTFS-RT (30 s), purge de l'historique (jour).
    /// L'état temps réel est publié dans Redis pour l'API : ConnectionStrings:Redis est obligatoire ici.
    /// </summary>
    internal static class Program
    {
        public static void Main(string[] p_Args)
        {
            HostApplicationBuilder v_Builder = Host.CreateApplicationBuilder(p_Args);
            string v_ConnectionString = v_Builder.Configuration.GetConnectionString(AppDbContext.ConnectionStringName)
                                        ?? throw new ConfigurationErrorsException($"{AppDbContext.ConnectionStringName} connection string can't be null and must be valid");

            v_Builder.Services.AddSerilog(p_Configuration => p_Configuration.ReadFrom.Configuration(v_Builder.Configuration));
            v_Builder.ConfigureContainer(new AutofacServiceProviderFactory(), static p_Builder =>
            {
                p_Builder.RegisterModule<CoreModule>();
                p_Builder.RegisterModule<InfrastructureModule>();
            });

            if (!v_Builder.Services.AddAmetisInfrastructure(v_Builder.Configuration, v_ConnectionString))
                throw new ConfigurationErrorsException("ConnectionStrings:Redis est obligatoire pour le Worker : sans Redis, l'API ne verrait pas le temps réel collecté");

            v_Builder.Services.AddCortexMediator([typeof(CoreModule)], p_Options => { p_Options.AddDefaultBehaviors(); p_Options.OnlyPublicClasses = false; });
            v_Builder.Services.AddHostedService<GtfsImportService>();
            v_Builder.Services.AddHostedService<RealtimeCollectorService>();
            v_Builder.Services.AddHostedService<VehicleHistoryPurgeService>();

            v_Builder.Build().Run();
        }
    }
}
