using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Domain.Interfaces.Network
{
    /// <summary>Lecture du réseau sur la version active du GTFS complet.</summary>
    public interface INetworkRepository
    {
        Task<IReadOnlyList<LineSummary>> SearchLinesAsync(string p_Query, CancellationToken p_CancellationToken);

        /// <summary>Null si la ligne n'existe pas.</summary>
        Task<LineDetail> GetLineAsync(string p_LineId, CancellationToken p_CancellationToken);

        Task<IReadOnlyList<LineShape>> GetLineShapesAsync(string p_LineId, CancellationToken p_CancellationToken);

        /// <summary>Recherche par nom et/ou par proximité (mètres). Sans critère : tous les arrêts.</summary>
        Task<IReadOnlyList<StopSummary>> SearchStopsAsync(string p_Query, double? p_Latitude, double? p_Longitude, int p_RadiusMeters, int p_Limit, CancellationToken p_CancellationToken);

        /// <summary>Null si l'arrêt n'existe pas.</summary>
        Task<StopDetail> GetStopAsync(string p_StopId, CancellationToken p_CancellationToken);

        Task<ServiceCalendarData> GetServiceCalendarAsync(CancellationToken p_CancellationToken);

        /// <summary>Passages des services donnés, hors arrêts sans montée (pickup_type = 1, ex. terminus).</summary>
        Task<IReadOnlyList<ScheduledStopTime>> GetStopTimesAsync(IReadOnlyCollection<string> p_StopIds, IReadOnlyCollection<string> p_ServiceIds, int p_FromSeconds, int p_ToSeconds, CancellationToken p_CancellationToken);
    }
}
