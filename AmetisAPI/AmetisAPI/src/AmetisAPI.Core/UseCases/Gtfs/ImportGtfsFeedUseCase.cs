using System.Security.Cryptography;
using AmetisAPI.Core.Gtfs;
using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Exceptions;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Gtfs;

namespace AmetisAPI.Core.UseCases.Gtfs
{
    /// <summary>
    /// Import d'un GTFS : téléchargement, empreinte, lecture, validation, écriture dans une nouvelle version, activation.
    /// Contenu identique à la version active : version journalisée « Skipped », rien n'est réécrit (pas de doublon).
    /// Échec : la version est marquée en échec et purgée, la version active reste en place.
    /// </summary>
    internal class ImportGtfsFeedUseCase(
        ILogger<ImportGtfsFeedUseCase> p_Logger,
        IUnitOfWork p_UnitOfWork,
        IGtfsSource p_Source,
        IGtfsParser p_Parser,
        IFeedRepository p_Feeds)
        : ADbUseCase<ImportGtfsFeedRequest, ValueResponse<FeedVersionInfo>>(p_Logger, p_UnitOfWork)
    {
        protected override Task<ValueResponse<FeedVersionInfo>> HandleLogic(ImportGtfsFeedRequest p_Request, CancellationToken p_CancellationToken)
        {
            return WithDatabaseAsync(() => ImportAsync(p_Request.Source, p_CancellationToken), p_CancellationToken);
        }

        private async Task<ValueResponse<FeedVersionInfo>> ImportAsync(FeedSource p_FeedSource, CancellationToken p_CancellationToken)
        {
            string v_Url = p_Source.GetUrl(p_FeedSource);
            m_Logger.LogInformation("Import GTFS {Source} : début", p_FeedSource);

            byte[] v_Content;
            try
            {
                v_Content = await p_Source.DownloadAsync(p_FeedSource, p_CancellationToken);
            }
            catch (Exception v_Ex) when (v_Ex is not OperationCanceledException)
            {
                int v_FailedId = await p_Feeds.StartVersionAsync(p_FeedSource, v_Url, string.Empty, CancellationToken.None);
                await p_Feeds.FailAsync(v_FailedId, $"Téléchargement impossible : {v_Ex.Message}", CancellationToken.None);
                m_Logger.LogError(v_Ex, "Import GTFS {Source} : téléchargement impossible", p_FeedSource);
                return ValueResponse<FeedVersionInfo>.Error(DomainErrors.Gtfs.DownloadFailed, "Téléchargement du GTFS impossible");
            }

            string v_Hash = Convert.ToHexString(SHA256.HashData(v_Content));
            FeedVersionInfo v_Active = await p_Feeds.GetActiveAsync(p_FeedSource, p_CancellationToken);
            int v_VersionId = await p_Feeds.StartVersionAsync(p_FeedSource, v_Url, v_Hash, p_CancellationToken);

            if (v_Active?.ContentHash == v_Hash)
            {
                await p_Feeds.MarkSkippedAsync(v_VersionId, p_CancellationToken);
                m_Logger.LogInformation("Import GTFS {Source} : contenu identique à la version {ActiveId}, ignoré", p_FeedSource, v_Active.Id);
                return new ValueResponse<FeedVersionInfo>(await p_Feeds.GetAsync(v_VersionId, p_CancellationToken), "Contenu identique à la version active");
            }

            try
            {
                using MemoryStream v_Stream = new(v_Content);
                GtfsFeed v_Feed = GtfsFeedValidator.Validate(p_Parser.Parse(v_Stream));
                await p_Feeds.WriteContentAsync(v_VersionId, v_Feed, p_CancellationToken);

                string v_Summary = v_Active is null
                    ? "Premier import"
                    : FeedChangeSummary.Build(
                        await p_Feeds.GetRouteIdsAsync(v_Active.Id, p_CancellationToken), v_Feed.Routes.Select(p_R => p_R.RouteId).ToList(),
                        await p_Feeds.GetStopIdsAsync(v_Active.Id, p_CancellationToken), v_Feed.Stops.Select(p_S => p_S.StopId).ToList());

                await p_Feeds.ActivateAsync(v_VersionId, p_FeedSource, v_Summary, p_CancellationToken);
                int v_Matches = await p_Feeds.RebuildMatchesAsync(p_CancellationToken);

                FeedVersionInfo v_Result = await p_Feeds.GetAsync(v_VersionId, p_CancellationToken);
                m_Logger.LogInformation(
                    "Import GTFS {Source} : version {VersionId} active, {Routes} lignes, {Stops} arrêts, {Trips} courses, {StopTimes} horaires, {Rejected} rejets, {Matches} correspondances. {Summary}",
                    p_FeedSource, v_VersionId, v_Result.RouteCount, v_Result.StopCount, v_Result.TripCount, v_Result.StopTimeCount, v_Result.RejectedCount, v_Matches, v_Summary);
                return new ValueResponse<FeedVersionInfo>(v_Result);
            }
            catch (Exception v_Ex)
            {
                // CancellationToken.None : la trace de l'échec doit être écrite même si l'import est annulé.
                await p_Feeds.FailAsync(v_VersionId, v_Ex.Message, CancellationToken.None);
                m_Logger.LogError(v_Ex, "Import GTFS {Source} : échec de la version {VersionId}, version active conservée", p_FeedSource, v_VersionId);
                return v_Ex is GtfsFormatException
                    ? ValueResponse<FeedVersionInfo>.Error(DomainErrors.Gtfs.InvalidFeed, v_Ex.Message)
                    : ValueResponse<FeedVersionInfo>.Error(DomainErrors.Gtfs.ImportFailed, "Échec de l'import GTFS");
            }
        }
    }
}
