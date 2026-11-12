using System.Configuration;
using System.Threading.RateLimiting;
using AmetisAPI.API.HealthChecks;
using AmetisAPI.API.Middlewares;
using AmetisAPI.API.Realtime;
using AmetisAPI.API.Swagger;
using AmetisAPI.Core;
using AmetisAPI.Core.Hosting;
using AmetisAPI.Infrastructure;
using AmetisAPI.Infrastructure.Configuration;
using AmetisAPI.Infrastructure.Services;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Cortex.Mediator.DependencyInjection;
using FluentValidation;
using HealthChecks.UI.Client;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace AmetisAPI.API
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static readonly TypeAdapterConfig m_TypeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        private const string HealthUrl = "/health";
        private const string AuthRateLimitPolicy = "auth";

        public static void Main(string[] p_Args)
        {
            // Outil : dotnet run --project src/AmetisAPI.API -- generate-vapid-keys (affiché à l'écran, jamais journalisé).
            if (p_Args.Contains("generate-vapid-keys"))
            {
                (string v_PublicKey, string v_PrivateKey) = AmetisAPI.Infrastructure.Push.VapidKeys.Generate();
                Console.WriteLine($"Push:PublicKey  = {v_PublicKey}{Environment.NewLine}Push:PrivateKey = {v_PrivateKey}");
                return;
            }

            WebApplicationBuilder v_Builder = WebApplication.CreateBuilder(p_Args);

            string v_ConnectionString = v_Builder.Configuration.GetConnectionString(AppDbContext.ConnectionStringName)
                                        ?? throw new ConfigurationErrorsException($"{AppDbContext.ConnectionStringName} connection string can't be null and must be valid");


            int v_MaxAllocatedMemory = int.Parse(v_Builder.Configuration["AllocatedMemory"]
                                                 ?? throw new ConfigurationErrorsException("AllocatedMemory can't be null and must be a valid integer"));
            string v_XmlCommentsFilePath =
                $"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}{typeof(Program).Assembly.GetName().Name}.xml";

            // Logger
            v_Builder.Host.UseSerilog((_, _, p_Configuration) =>
                p_Configuration.ReadFrom.Configuration(v_Builder.Configuration));

            // Autofac for module registration
            v_Builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            v_Builder.Host.ConfigureContainer<ContainerBuilder>(static p_Builder =>
            {
                p_Builder.RegisterModule<ApiModule>(); //from API itself
                p_Builder.RegisterModule<CoreModule>(); //from core library
                p_Builder.RegisterModule<InfrastructureModule>(); //from infrastructure library
            });

            // EF Core, flux Ametis, état temps réel (Redis si configuré)
            bool v_HasRedis = v_Builder.Services.AddAmetisInfrastructure(v_Builder.Configuration, v_ConnectionString);

            // Health check
            v_Builder.Services.AddHealthChecks()
                .AddProcessAllocatedMemoryHealthCheck(v_MaxAllocatedMemory, name: "Allocated Memory",
                    tags: ["allocatedmemory", "memory"])
                .AddDbContextCheck<AppDbContext>(name: "EF DbContext", tags: new[] { "ef", "dbcontext" })
                .AddSqlServer(v_ConnectionString, healthQuery: "SELECT 1;", name: "Sql Server",
                    tags: new[] { "db", "sql", "sqlserver" })
                .AddCheck<DataHealthCheck>("Données Ametis", tags: ["gtfs", "realtime"])
                ;

            bool v_EmailEnabled = v_Builder.Configuration.GetSection(SmtpOptions.Section).Get<SmtpOptions>()?.IsConfigured ?? false;

            // Comptes : ASP.NET Core Identity, cookie HttpOnly + Secure + SameSite=Strict (protection CSRF).
            v_Builder.Services.AddIdentityApiEndpoints<AppUser>(p_Options =>
                {
                    p_Options.User.RequireUniqueEmail = true;
                    // Confirmation exigée seulement si les e-mails peuvent partir (section Smtp renseignée).
                    p_Options.SignIn.RequireConfirmedEmail = v_EmailEnabled;
                    p_Options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddErrorDescriber<FrenchIdentityErrorDescriber>();
            v_Builder.Services.AddAmetisEmail(v_Builder.Configuration);
            v_Builder.Services.ConfigureApplicationCookie(p_Options =>
            {
                p_Options.Cookie.Name = "ametis.auth";
                p_Options.Cookie.HttpOnly = true;
                p_Options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                p_Options.Cookie.SameSite = SameSiteMode.Strict;
                p_Options.ExpireTimeSpan = TimeSpan.FromDays(14);
                p_Options.SlidingExpiration = true;
                p_Options.Events.OnRedirectToLogin = p_Context =>
                {
                    p_Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                p_Options.Events.OnRedirectToAccessDenied = p_Context =>
                {
                    p_Context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });
            v_Builder.Services.AddAuthorization();

            // Clés des cookies et des jetons de compte : hors du conteneur, sinon chaque redémarrage déconnecte tout le monde.
            string v_KeysPath = v_Builder.Configuration["DataProtection:KeysPath"];
            if (!string.IsNullOrWhiteSpace(v_KeysPath))
            {
                v_Builder.Services.AddDataProtection()
                    .SetApplicationName("Nautibus")
                    .PersistKeysToFileSystem(new DirectoryInfo(v_KeysPath));
            }

            // Limitation de débit des endpoints d'authentification, par adresse IP.
            v_Builder.Services.AddRateLimiter(p_Options =>
            {
                p_Options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                p_Options.AddPolicy(AuthRateLimitPolicy, p_Context => RateLimitPartition.GetFixedWindowLimiter(
                    p_Context.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
            });

            v_Builder.Services.AddControllers()
                .AddJsonOptions(p_Options =>
                    // To enable display of enums as string
                    p_Options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
                );
            v_Builder.Services.AddSignalR()
                .AddJsonProtocol(p_Options => p_Options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            v_Builder.Services.AddEndpointsApiExplorer();
            v_Builder.Services.AddApiVersioning(p_O =>
                {
                    p_O.AssumeDefaultVersionWhenUnspecified = true;
                    p_O.DefaultApiVersion = new ApiVersion(1, 0);
                    p_O.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(p_O =>
                {
                    p_O.GroupNameFormat = "'v'VVV";
                    p_O.SubstituteApiVersionInUrl = true;
                });
            v_Builder.Services.AddSwaggerGen(p_X =>
                // integrate xml comments
                p_X.IncludeXmlComments(v_XmlCommentsFilePath));
            v_Builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

            v_Builder.Services.AddValidatorsFromAssemblyContaining<ApiModule>();
            v_Builder.Services.AddCortexMediator([typeof(CoreModule)], p_Options => { p_Options.AddDefaultBehaviors(); p_Options.OnlyPublicClasses = false; });

            // CORS : uniquement les origines du SPA déclarées en configuration (cookies d'authentification).
            string[] v_Origins = v_Builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            v_Builder.Services.AddCors(p_Options => p_Options.AddDefaultPolicy(p_Policy => p_Policy
                .WithOrigins(v_Origins)
                .WithExposedHeaders("Content-Disposition")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()));

            v_Builder.Services.AddMapster();
            v_Builder.Services.AddSingleton(p_Sp =>
            {
                TypeAdapterConfig v_Config = m_TypeAdapterConfig;
                v_Config.Scan(typeof(ApiModule).Assembly, typeof(InfrastructureModule).Assembly);
                return v_Config;
            });

            // Derrière le proxy HTTPS (Nginx Proxy Manager, puis nginx du service web) : adresse IP et schéma réels du client. Le port de l'API n'est joignable
            // que depuis le réseau Docker interne, d'où la confiance accordée à tout proxy de ce réseau.
            bool v_BehindProxy = v_Builder.Configuration.GetValue<bool>("ReverseProxy:Enabled");
            if (v_BehindProxy)
            {
                v_Builder.Services.Configure<ForwardedHeadersOptions>(p_Options =>
                {
                    p_Options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                    p_Options.KnownIPNetworks.Clear();
                    p_Options.KnownProxies.Clear();
                });
            }

            // Temps réel : diffusion SignalR ; collecte et imports dans le Worker, ou ici en mode processus unique.
            v_Builder.Services.AddHostedService<VehicleBroadcastService>();
            bool v_CollectInProcess = v_Builder.Configuration.GetValue<bool>("Realtime:CollectInProcess");
            if (v_CollectInProcess)
            {
                v_Builder.Services.AddHostedService<GtfsImportService>();
                v_Builder.Services.AddHostedService<RealtimeCollectorService>();
                v_Builder.Services.AddHostedService<VehicleHistoryPurgeService>();
            }

            WebApplication v_App = v_Builder.Build();

            if (!v_EmailEnabled)
                v_App.Logger.LogWarning("Section Smtp vide : aucun e-mail envoyé, confirmation d'adresse non exigée");
            if (!v_HasRedis && !v_CollectInProcess)
                v_App.Logger.LogWarning("Ni Redis ni collecte dans le processus : aucun temps réel ne sera disponible");

            // Configure the HTTP request pipeline.
            if (v_App.Environment.IsDevelopment())
            {
                v_App.UseSwagger();
                v_App.UseSwaggerUI();
            }
            else
            {
                v_App.UseHsts();
            }

            if (v_BehindProxy)
                v_App.UseForwardedHeaders();
            v_App.UseMiddleware<HandleExceptionMiddleware>();
            // Derrière le proxy, HTTPS est assuré par celui-ci.
            if (!v_BehindProxy)
                v_App.UseHttpsRedirection();
            v_App.UseCors();
            v_App.UseRateLimiter();

            v_App.UseAuthentication();
            v_App.UseAuthorization();
            v_App.UseSerilogRequestLogging();


            v_App.MapControllers();
            v_App.MapGroup("/api/v1/auth").MapIdentityApi<AppUser>().RequireRateLimiting(AuthRateLimitPolicy);
            v_App.MapHub<VehicleHub>(VehicleHub.Route);
            v_App.MapHealthChecks(HealthUrl, new HealthCheckOptions
            {
                AllowCachingResponses = false,
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }).AllowAnonymous();

            v_App.Run();
        }
    }
}
