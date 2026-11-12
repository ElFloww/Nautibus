using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Historique des positions de véhicules (données publiques), conservé 30 jours.</summary>
    public class VehiclePositionEntity : IEntity
    {
        public long Id { get; set; }
        public string VehicleId { get; set; }
        public string TripId { get; set; }
        public string RouteId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Bearing { get; set; }
        public double? Speed { get; set; }
        public DateTime PositionTimestampUtc { get; set; }
    }
}
