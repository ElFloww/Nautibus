using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Realtime
{
    internal static class RealtimeStatusFactory
    {
        public static RealtimeStatus From(RealtimeSnapshot p_Snapshot, DateTime p_NowUtc)
        {
            if (p_Snapshot is null)
                return new RealtimeStatus(null, null, null, true, 0, 0, ["Aucune collecte temps réel effectuée"]);

            int? v_Age = p_Snapshot.FeedTimestampUtc is { } v_Ts ? RealtimeRules.AgeSeconds(p_NowUtc, v_Ts) : null;
            return new RealtimeStatus(
                p_Snapshot.FeedTimestampUtc,
                p_Snapshot.CollectedAtUtc,
                v_Age,
                v_Age is null || v_Age > RealtimeRules.StaleAfterSeconds,
                p_Snapshot.Vehicles.Count(p_V => RealtimeRules.AgeSeconds(p_NowUtc, p_V.TimestampUtc) <= RealtimeRules.HideAfterSeconds),
                p_Snapshot.Alerts.Count,
                p_Snapshot.Errors);
        }
    }
}
