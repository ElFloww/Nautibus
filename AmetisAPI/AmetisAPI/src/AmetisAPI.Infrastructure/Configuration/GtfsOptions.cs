namespace AmetisAPI.Infrastructure.Configuration
{
    /// <summary>Adresses des flux Ametis (section « Gtfs » de la configuration). Voir docs/P0-audit-donnees.md.</summary>
    public class GtfsOptions
    {
        public const string Section = "Gtfs";

        public string NetworkFeedUrl { get; set; }
        public string RealtimeFeedUrl { get; set; }
        public string VehiclePositionsUrl { get; set; }
        public string TripUpdatesUrl { get; set; }
        public string AlertsUrl { get; set; }
    }
}
