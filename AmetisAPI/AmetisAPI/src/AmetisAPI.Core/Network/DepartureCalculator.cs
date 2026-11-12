using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.Network
{
    /// <summary>Passages théoriques d'un arrêt : services du jour, plus ceux de la veille dont les horaires dépassent 24 h.</summary>
    internal static class DepartureCalculator
    {
        public static async Task<List<Departure>> ComputeAsync(INetworkRepository p_Network, StopDetail p_Stop, DateTimeOffset p_From, int p_WindowMinutes, CancellationToken p_CancellationToken)
        {
            List<string> v_StopIds = [p_Stop.Stop.Id, .. p_Stop.Quays.Select(p_Q => p_Q.Id)];
            DateOnly v_Today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(p_From, NetworkTime.m_Paris).DateTime);
            int v_Window = p_WindowMinutes * 60;
            ServiceCalendarData v_Calendar = await p_Network.GetServiceCalendarAsync(p_CancellationToken);

            List<Departure> v_Departures = [];
            foreach (DateOnly v_ServiceDate in new[] { v_Today.AddDays(-1), v_Today })
            {
                HashSet<string> v_Services = ServiceCalendar.ActiveServiceIds(v_Calendar, v_ServiceDate);
                // Pour la veille, l'origine dépasse 24 h : seules les courses après minuit peuvent répondre.
                if (v_Services.Count == 0)
                    continue;
                int v_FromSeconds = NetworkTime.SecondsSince(v_ServiceDate, p_From);
                IReadOnlyList<ScheduledStopTime> v_StopTimes = await p_Network.GetStopTimesAsync(v_StopIds, v_Services, v_FromSeconds, v_FromSeconds + v_Window, p_CancellationToken);
                v_Departures.AddRange(v_StopTimes.Select(p_S => new Departure(
                    p_S.TripId, p_S.StopId, p_S.Line, p_S.Destination, p_S.DirectionId, NetworkTime.At(v_ServiceDate, p_S.DepartureSeconds))));
            }
            return v_Departures.OrderBy(p_D => p_D.ScheduledTime).ThenBy(p_D => p_D.Line.SortOrder).ToList();
        }
    }
}
