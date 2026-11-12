using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class FeedVersionEntity : IIdEntity
    {
        public int Id { get; set; }
        public byte Source { get; set; }
        public byte Status { get; set; }
        public bool IsActive { get; set; }
        public string SourceUrl { get; set; }
        public string ContentHash { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime? FinishedAtUtc { get; set; }
        public int RouteCount { get; set; }
        public int StopCount { get; set; }
        public int TripCount { get; set; }
        public int StopTimeCount { get; set; }
        public int ShapePointCount { get; set; }
        public int RejectedCount { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }
        public string ChangeSummary { get; set; }
        public string ErrorMessage { get; set; }
    }
}
