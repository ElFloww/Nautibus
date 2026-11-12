using AmetisAPI.Domain.Interfaces.Gtfs;
using AmetisAPI.Domain.Interfaces.Push;
using AmetisAPI.Infrastructure.Push;
using AmetisAPI.Domain.Interfaces.Realtime;
using AmetisAPI.Infrastructure.Configuration;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using AmetisAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AmetisAPI.Infrastructure
{
    /// <summary>Enregistrements partagés par l'API et le Worker (base, flux HTTP, stockage temps réel).</summary>
    public static class InfrastructureServiceCollectionExtensions
    {
        public const string RedisConnectionStringName = "Redis";

        /// <summary>Retourne true si Redis est configuré ; sinon l'état temps réel reste en mémoire du processus.</summary>
        public static bool AddAmetisInfrastructure(this IServiceCollection p_Services, IConfiguration p_Configuration, string p_ConnectionString)
        {
            p_Services.AddDbContext<AppDbContext>(p_Options => p_Options.UseSqlServer(p_ConnectionString, p_Sql => p_Sql
                .UseNetTopologySuite()
                .CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds)
                .EnableRetryOnFailure()));

            p_Services.Configure<GtfsOptions>(p_Configuration.GetSection(GtfsOptions.Section));
            p_Services.AddHttpClient<HttpGtfsSource>(p_Client => p_Client.Timeout = TimeSpan.FromSeconds(60));
            p_Services.AddTransient<IGtfsSource>(p_Sp => p_Sp.GetRequiredService<HttpGtfsSource>());
            p_Services.AddTransient<IGtfsRealtimeSource>(p_Sp => p_Sp.GetRequiredService<HttpGtfsSource>());

            p_Services.Configure<PushOptions>(p_Configuration.GetSection(PushOptions.Section));
            p_Services.AddHttpClient<WebPushSender>(p_Client => p_Client.Timeout = TimeSpan.FromSeconds(15));
            p_Services.AddTransient<IPushSender>(p_Sp => p_Sp.GetRequiredService<WebPushSender>());

            string v_Redis = p_Configuration.GetConnectionString(RedisConnectionStringName);
            if (string.IsNullOrWhiteSpace(v_Redis))
            {
                p_Services.AddSingleton<IRealtimeStore, InMemoryRealtimeStore>();
                return false;
            }
            p_Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(v_Redis));
            p_Services.AddSingleton<IRealtimeStore, RedisRealtimeStore>();
            return true;
        }

        /// <summary>Envoi SMTP des e-mails de compte si la section « Smtp » est renseignée. Retourne true si l'envoi est actif.</summary>
        public static bool AddAmetisEmail(this IServiceCollection p_Services, IConfiguration p_Configuration)
        {
            IConfigurationSection v_Section = p_Configuration.GetSection(SmtpOptions.Section);
            p_Services.Configure<SmtpOptions>(v_Section);
            SmtpOptions v_Options = v_Section.Get<SmtpOptions>() ?? new SmtpOptions();
            if (!v_Options.IsConfigured)
                return false;
            if (string.IsNullOrWhiteSpace(v_Options.FromAddress))
                throw new InvalidOperationException("Smtp:FromAddress est obligatoire quand Smtp:Host est renseigné");
            // Remplace l'expéditeur sans effet enregistré par Identity.
            p_Services.AddTransient<IEmailSender<AppUser>, SmtpEmailSender>();
            return true;
        }
    }
}
