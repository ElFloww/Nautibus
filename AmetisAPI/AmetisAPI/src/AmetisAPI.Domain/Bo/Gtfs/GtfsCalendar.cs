namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>Jours actifs : bit 0 = lundi ... bit 6 = dimanche.</summary>
    public record GtfsCalendar(string ServiceId, int DaysMask, DateOnly StartDate, DateOnly EndDate);
}
