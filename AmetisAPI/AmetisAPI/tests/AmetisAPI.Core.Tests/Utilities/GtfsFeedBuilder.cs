using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Core.Tests.Utilities
{
    /// <summary>Petit réseau cohérent : 1 ligne, 3 arrêts (dont une zone), 2 courses, 1 service en semaine.</summary>
    internal static class GtfsFeedBuilder
    {
        public static GtfsFeed Valid() => new()
        {
            Routes = [new GtfsRoute("L1", "1", "Gare <> CHU", 3, "E3C305", "17202A", 1)],
            Stops =
            [
                new GtfsStop("AREA", null, "Gare", 49.89, 2.30, 1, null, 0),
                new GtfsStop("Q1", null, "Gare", 49.8901, 2.3001, 0, "AREA", 0),
                new GtfsStop("Q2", null, "CHU", 49.87, 2.25, 0, null, 0)
            ],
            Trips =
            [
                new GtfsTrip("T1", "L1", "SEM", "Aller", 0, "S1"),
                new GtfsTrip("T2", "L1", "SEM", "Retour", 1, "S2")
            ],
            StopTimes =
            [
                new GtfsStopTime("T1", 1, "Q1", 28800, 28800, 0, 1, null),
                new GtfsStopTime("T1", 2, "Q2", 29400, 29400, 1, 0, null),
                new GtfsStopTime("T2", 1, "Q2", 30000, 30000, 0, 1, null),
                new GtfsStopTime("T2", 2, "Q1", 30600, 30600, 1, 0, null)
            ],
            Calendars = [new GtfsCalendar("SEM", 0b0011111, new DateOnly(2026, 11, 1), new DateOnly(2027, 5, 1))],
            CalendarDates = [new GtfsCalendarDate("SEM", new DateOnly(2026, 11, 11), 2)]
        };
    }
}
