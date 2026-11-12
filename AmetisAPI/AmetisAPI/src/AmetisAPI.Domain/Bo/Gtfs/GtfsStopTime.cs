namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>Horaires en secondes depuis minuit du jour de service (peuvent dépasser 24 h).</summary>
    public record GtfsStopTime(string TripId, int StopSequence, string StopId, int? ArrivalSeconds, int? DepartureSeconds, int PickupType, int DropOffType, double? ShapeDistTraveled);
}
