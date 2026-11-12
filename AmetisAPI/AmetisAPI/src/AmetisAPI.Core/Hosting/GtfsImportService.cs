using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using Microsoft.Extensions.DependencyInjection;

namespace AmetisAPI.Core.Hosting
{
    /// <summary>
    /// Import des deux GTFS au démarrage puis toutes les 6 h. Un contenu inchangé est ignoré (empreinte SHA-256),
    /// donc la fréquence ne crée pas de doublon ; elle rattrape la republication hebdomadaire du GTFS temps réel.
    /// </summary>
    public class GtfsImportService(IServiceScopeFactory p_ScopeFactory, ILogger<GtfsImportService> p_Logger)
        : ScheduledMediatorService(p_ScopeFactory, p_Logger)
    {
        protected override TimeSpan Interval => TimeSpan.FromHours(6);

        protected override async Task RunOnceAsync(IMediator p_Mediator, CancellationToken p_CancellationToken)
        {
            foreach (FeedSource v_Source in new[] { FeedSource.Network, FeedSource.Realtime })
            {
                ValueResponse<FeedVersionInfo> v_Result = await p_Mediator.SendQueryAsync<ImportGtfsFeedRequest, ValueResponse<FeedVersionInfo>>(new ImportGtfsFeedRequest(v_Source), p_CancellationToken);
                if (!v_Result.Success)
                    m_Logger.LogError("Import GTFS {Source} en échec : {Errors}", v_Source, string.Join(" ; ", v_Result.Errors.Select(p_E => $"{p_E.Code} {p_E.Description}")));
            }
        }
    }
}
