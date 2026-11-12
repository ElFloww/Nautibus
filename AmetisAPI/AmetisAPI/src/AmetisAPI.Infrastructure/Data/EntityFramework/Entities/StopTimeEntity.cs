using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class StopTimeEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string TripId { get; set; }
        public int StopSequence { get; set; }
        public string StopId { get; set; }
        public int? ArrivalSeconds { get; set; }
        public int? DepartureSeconds { get; set; }
        public int PickupType { get; set; }
        public int DropOffType { get; set; }
        public double? ShapeDistTraveled { get; set; }
    }
}
