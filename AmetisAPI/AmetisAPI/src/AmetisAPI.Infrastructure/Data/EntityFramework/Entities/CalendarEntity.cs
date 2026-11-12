using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class CalendarEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string ServiceId { get; set; }
        public int DaysMask { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
