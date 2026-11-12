using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Domain.Bo.Network
{
    public record ServiceCalendarData(IReadOnlyList<GtfsCalendar> Calendars, IReadOnlyList<GtfsCalendarDate> CalendarDates);
}
