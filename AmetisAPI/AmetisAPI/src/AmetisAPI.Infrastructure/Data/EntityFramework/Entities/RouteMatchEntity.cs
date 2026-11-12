using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class RouteMatchEntity : IEntity
    {
        public string RealtimeRouteId { get; set; }
        public string NetworkRouteId { get; set; }
        public byte Confidence { get; set; }
    }
}
