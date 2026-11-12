using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using NetTopologySuite.Geometries;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class StopEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string StopId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        /// <summary>Calculé à l'import depuis Latitude/Longitude (SRID 4326) pour la recherche par proximité.</summary>
        public Point Location { get; set; }
        public int LocationType { get; set; }
        public string ParentStationId { get; set; }
        public int WheelchairBoarding { get; set; }
    }
}
