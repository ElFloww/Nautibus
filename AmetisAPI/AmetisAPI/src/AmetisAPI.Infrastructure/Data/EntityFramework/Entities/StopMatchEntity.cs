using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Arrêt du flux temps réel rattaché à l'arrêt le plus proche du réseau de référence.</summary>
    public class StopMatchEntity : IEntity
    {
        public string RealtimeStopId { get; set; }
        public string NetworkStopId { get; set; }
        public double DistanceMeters { get; set; }
        public byte Confidence { get; set; }
    }
}
