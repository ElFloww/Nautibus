using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    /// <summary>Favori d'un utilisateur ; supprimé en cascade avec le compte.</summary>
    public class FavoriteEntity : IIdEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public byte Kind { get; set; }
        public string TargetId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
