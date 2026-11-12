using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Interfaces.Gtfs;
using AmetisAPI.Domain.Interfaces.Realtime;
using AmetisAPI.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AmetisAPI.Infrastructure.Services
{
    /// <summary>Téléchargement HTTP des flux statiques et temps réel (sans authentification d'après l'audit P0).</summary>
    internal class HttpGtfsSource(HttpClient p_Http, IOptions<GtfsOptions> p_Options) : IGtfsSource, IGtfsRealtimeSource
    {
        private readonly GtfsOptions m_Options = p_Options.Value;

        public string GetUrl(FeedSource p_Source) => p_Source switch
        {
            FeedSource.Network => m_Options.NetworkFeedUrl,
            FeedSource.Realtime => m_Options.RealtimeFeedUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(p_Source))
        };

        public Task<byte[]> DownloadAsync(FeedSource p_Source, CancellationToken p_CancellationToken) => GetAsync(GetUrl(p_Source), p_CancellationToken);

        public Task<byte[]> FetchVehiclePositionsAsync(CancellationToken p_CancellationToken) => GetAsync(m_Options.VehiclePositionsUrl, p_CancellationToken);

        public Task<byte[]> FetchTripUpdatesAsync(CancellationToken p_CancellationToken) => GetAsync(m_Options.TripUpdatesUrl, p_CancellationToken);

        public Task<byte[]> FetchAlertsAsync(CancellationToken p_CancellationToken) => GetAsync(m_Options.AlertsUrl, p_CancellationToken);

        private async Task<byte[]> GetAsync(string p_Url, CancellationToken p_CancellationToken)
        {
            if (string.IsNullOrWhiteSpace(p_Url))
                throw new InvalidOperationException("Adresse de flux non configurée (section Gtfs)");
            using HttpResponseMessage v_Response = await p_Http.GetAsync(p_Url, p_CancellationToken);
            v_Response.EnsureSuccessStatusCode();
            return await v_Response.Content.ReadAsByteArrayAsync(p_CancellationToken);
        }
    }
}
