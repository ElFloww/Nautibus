using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Network
{
    /// <summary>Services actifs un jour donné (calendar.txt puis exceptions de calendar_dates.txt).</summary>
    internal static class ServiceCalendar
    {
        public static HashSet<string> ActiveServiceIds(ServiceCalendarData p_Data, DateOnly p_Date)
        {
            int v_DayBit = ((int)p_Date.DayOfWeek + 6) % 7; // lundi = 0
            HashSet<string> v_Active = p_Data.Calendars
                .Where(p_C => p_C.StartDate <= p_Date && p_Date <= p_C.EndDate && ((p_C.DaysMask >> v_DayBit) & 1) == 1)
                .Select(p_C => p_C.ServiceId)
                .ToHashSet();

            foreach (Domain.Bo.Gtfs.GtfsCalendarDate v_Exception in p_Data.CalendarDates.Where(p_D => p_D.Date == p_Date))
            {
                if (v_Exception.ExceptionType == 1)
                    v_Active.Add(v_Exception.ServiceId);
                else if (v_Exception.ExceptionType == 2)
                    v_Active.Remove(v_Exception.ServiceId);
            }
            return v_Active;
        }
    }
}
