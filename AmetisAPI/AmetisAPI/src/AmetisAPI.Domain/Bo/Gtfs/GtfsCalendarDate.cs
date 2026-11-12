namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>ExceptionType : 1 = service ajouté, 2 = service retiré.</summary>
    public record GtfsCalendarDate(string ServiceId, DateOnly Date, int ExceptionType);
}
