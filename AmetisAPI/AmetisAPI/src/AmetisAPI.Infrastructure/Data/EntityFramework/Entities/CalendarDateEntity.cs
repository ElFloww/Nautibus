using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class CalendarDateEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string ServiceId { get; set; }
        public DateOnly Date { get; set; }
        public int ExceptionType { get; set; }
    }
}
