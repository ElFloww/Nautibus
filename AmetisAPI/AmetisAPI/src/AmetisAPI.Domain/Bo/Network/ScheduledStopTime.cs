namespace AmetisAPI.Domain.Bo.Network
{
    /// <summary>Passage théorique brut lu en base, avant conversion en heure locale.</summary>
    public record ScheduledStopTime(string TripId, string ServiceId, string StopId, int DepartureSeconds, int? DirectionId, string Destination, LineSummary Line);
}
