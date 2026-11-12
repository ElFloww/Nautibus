namespace AmetisAPI.Domain.Bo.Network
{
    public record Departure(string TripId, string StopId, LineSummary Line, string Destination, int? DirectionId, DateTimeOffset ScheduledTime);
}
