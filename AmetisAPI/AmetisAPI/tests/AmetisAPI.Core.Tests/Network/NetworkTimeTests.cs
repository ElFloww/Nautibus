using AmetisAPI.Core.Network;

namespace AmetisAPI.Core.Tests.Network
{
    public class NetworkTimeTests
    {
        [Fact]
        public void Time_Beyond_24h_Falls_On_Next_Calendar_Day()
        {
            DateTimeOffset v_Time = NetworkTime.At(new DateOnly(2026, 11, 12), 25 * 3600 + 30 * 60);

            Check.That(v_Time).IsEqualTo(new DateTimeOffset(2026, 11, 13, 1, 30, 0, TimeSpan.FromHours(1)));
        }

        [Fact]
        public void Daylight_Saving_Day_Uses_Noon_Minus_12h_Reference()
        {
            // 28 mars 2027 : passage à l'heure d'été. 08:00:00 GTFS = 08:00 heure d'été (UTC+2).
            DateTimeOffset v_Time = NetworkTime.At(new DateOnly(2027, 3, 28), 8 * 3600);

            Check.That(v_Time).IsEqualTo(new DateTimeOffset(2027, 3, 28, 8, 0, 0, TimeSpan.FromHours(2)));
        }

        [Fact]
        public void Seconds_Since_Is_Inverse_Of_At()
        {
            DateOnly v_Day = new(2026, 11, 12);
            Check.That(NetworkTime.SecondsSince(v_Day, NetworkTime.At(v_Day, 40000))).IsEqualTo(40000);
        }
    }
}
