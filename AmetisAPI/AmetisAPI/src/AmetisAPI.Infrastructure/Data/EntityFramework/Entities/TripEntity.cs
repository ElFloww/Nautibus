using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Entities
{
    public class TripEntity : IEntity
    {
        public int FeedVersionId { get; set; }
        public string TripId { get; set; }
        public string RouteId { get; set; }
        public string ServiceId { get; set; }

        /// <summary>trip_headsign tel que publié (« Aller »/« Retour » dans le GTFS complet).</summary>
        public string Headsign { get; set; }

        /// <summary>Nom du dernier arrêt de la course, calculé à l'import (affichage de la destination).</summary>
        public string Destination { get; set; }
        public int? DirectionId { get; set; }
        public string ShapeId { get; set; }
    }
}
