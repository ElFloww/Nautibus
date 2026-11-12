using AmetisAPI.Core.Network;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Tests.Network
{
    public class ServiceCalendarTests
    {
        private static readonly ServiceCalendarData m_Data = new(
            [
                new GtfsCalendar("SEMAINE", 0b0011111, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)),
                new GtfsCalendar("DIMANCHE", 0b1000000, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30))
            ],
            [
                new GtfsCalendarDate("SEMAINE", new DateOnly(2026, 11, 11), 2),
                new GtfsCalendarDate("DIMANCHE", new DateOnly(2026, 11, 11), 1),
                new GtfsCalendarDate("SPECIAL", new DateOnly(2026, 12, 24), 1)
            ]);

        [Theory]
        [InlineData(2026, 11, 12, "SEMAINE")] // jeudi
        [InlineData(2026, 11, 15, "DIMANCHE")] // dimanche
        [InlineData(2026, 11, 11, "DIMANCHE")] // férié : semaine retirée, dimanche ajouté
        [InlineData(2026, 12, 24, "SPECIAL")] // service défini uniquement par calendar_dates
        public void Active_Services_Follow_Weekday_Range_And_Exceptions(int p_Y, int p_M, int p_D, string p_Expected)
        {
            Check.That(ServiceCalendar.ActiveServiceIds(m_Data, new DateOnly(p_Y, p_M, p_D))).ContainsExactly(p_Expected);
        }

        [Fact]
        public void No_Service_Outside_Calendar_Range()
        {
            Check.That(ServiceCalendar.ActiveServiceIds(m_Data, new DateOnly(2026, 12, 1))).IsEmpty();
        }
    }
}
