using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Passage publié par Ametis (identifiants du flux temps réel). Clé fonctionnelle : course, arrêt, heure prévue.</summary>
    public class StopPassageEntity : IEntity
    {
        public long Id { get; set; }
        public string TripId { get; set; }
        public string RouteId { get; set; }
        public string StopId { get; set; }
        public int StopOrder { get; set; }
        public DateTime ScheduledUtc { get; set; }
        public DateTime ExpectedUtc { get; set; }
        public int DelaySeconds { get; set; }
        public string VehicleId { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
