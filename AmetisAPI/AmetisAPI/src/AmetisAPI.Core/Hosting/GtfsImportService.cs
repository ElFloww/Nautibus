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
        /// <summary>Après un import en échec (téléchargement coupé, fichier invalide), nouvel essai rapproché.</summary>
        public static readonly TimeSpan m_RetryDelay = TimeSpan.FromMinutes(10);

        private bool m_LastRunFailed;

        protected override TimeSpan Interval => TimeSpan.FromHours(6);

        protected internal override TimeSpan NextDelay => m_LastRunFailed ? m_RetryDelay : Interval;

        protected internal override async Task RunOnceAsync(IMediator p_Mediator, CancellationToken p_CancellationToken)
        {
            m_LastRunFailed = false;
            foreach (FeedSource v_Source in new[] { FeedSource.Network, FeedSource.Realtime })
            {
                ValueResponse<FeedVersionInfo> v_Result = await p_Mediator.SendQueryAsync<ImportGtfsFeedRequest, ValueResponse<FeedVersionInfo>>(new ImportGtfsFeedRequest(v_Source), p_CancellationToken);
                if (v_Result.Success)
                    continue;
                m_LastRunFailed = true;
                m_Logger.LogError("Import GTFS {Source} en échec : {Errors}", v_Source, string.Join(" ; ", v_Result.Errors.Select(p_E => $"{p_E.Code} {p_E.Description}")));
            }
        }
    }
}
