using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Lignes desservant chaque arrêt, calculé à l'import depuis stop_times.</summary>
    public class StopRouteEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string StopId { get; set; }
        public string RouteId { get; set; }
    }
}
