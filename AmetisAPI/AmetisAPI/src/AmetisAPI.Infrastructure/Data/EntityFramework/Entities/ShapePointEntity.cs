using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class ShapePointEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string ShapeId { get; set; }
        public int Sequence { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
