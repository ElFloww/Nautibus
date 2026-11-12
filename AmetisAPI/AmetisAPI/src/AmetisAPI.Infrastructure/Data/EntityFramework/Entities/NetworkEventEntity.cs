using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Évènement réseau (alerte officielle ou déviation détectée). LineIds : identifiants de lignes en JSON.</summary>
    public class NetworkEventEntity : IIdEntity
    {
        public int Id { get; set; }
        public byte Kind { get; set; }
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string LineIds { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
        public DateTime? ClosedUtc { get; set; }
        public DateTime LastSeenUtc { get; set; }
        public DateTime CreatedUtc { get; set; }
        public byte Confidence { get; set; }
    }
}
