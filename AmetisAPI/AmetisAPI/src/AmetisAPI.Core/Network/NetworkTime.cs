namespace AmetisAPI.Core.Network
{
    /// <summary>Conversion des heures GTFS (secondes depuis « midi moins 12 h » du jour de service, heure de Paris).</summary>
    internal static class NetworkTime
    {
        public static readonly TimeZoneInfo m_Paris = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");

        public static DateTimeOffset ToParis(DateTime p_UtcNow) => TimeZoneInfo.ConvertTime(new DateTimeOffset(p_UtcNow, TimeSpan.Zero), m_Paris);

        /// <summary>Référence GTFS du jour : midi local moins 12 h (exact aussi les jours de changement d'heure).</summary>
        public static DateTimeOffset ServiceDayReference(DateOnly p_ServiceDate)
        {
            DateTime v_Noon = p_ServiceDate.ToDateTime(new TimeOnly(12, 0));
            return new DateTimeOffset(v_Noon, m_Paris.GetUtcOffset(v_Noon)).AddHours(-12);
        }

        public static DateTimeOffset At(DateOnly p_ServiceDate, int p_Seconds) => TimeZoneInfo.ConvertTime(ServiceDayReference(p_ServiceDate).AddSeconds(p_Seconds), m_Paris);

        /// <summary>Secondes écoulées depuis la référence du jour de service.</summary>
        public static int SecondsSince(DateOnly p_ServiceDate, DateTimeOffset p_Instant) => (int)(p_Instant - ServiceDayReference(p_ServiceDate)).TotalSeconds;
    }
}
