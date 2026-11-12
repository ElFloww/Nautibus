using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class RouteEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string RouteId { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public int RouteType { get; set; }
        public string Color { get; set; }
        public string TextColor { get; set; }
        public int? SortOrder { get; set; }
    }
}
