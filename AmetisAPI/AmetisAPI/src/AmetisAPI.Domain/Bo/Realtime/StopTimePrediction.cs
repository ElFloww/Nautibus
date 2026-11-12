namespace AmetisAPI.Domain.Bo.Realtime
{
    public record StopTimePrediction(string TripId, string RouteId, string StopId, int? StopSequence, DateTime? ArrivalUtc, DateTime? DepartureUtc, int? DelaySeconds, string VehicleId);
}
