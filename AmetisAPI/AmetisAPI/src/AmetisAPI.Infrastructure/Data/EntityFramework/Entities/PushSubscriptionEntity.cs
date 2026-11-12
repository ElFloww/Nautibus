using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Abonnement Web Push d'un navigateur ; supprimé en cascade avec le compte. EndpointHash : SHA-256 de l'adresse (index unique).</summary>
    public class PushSubscriptionEntity : IIdEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Endpoint { get; set; }
        public string EndpointHash { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
        public bool AllLines { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
