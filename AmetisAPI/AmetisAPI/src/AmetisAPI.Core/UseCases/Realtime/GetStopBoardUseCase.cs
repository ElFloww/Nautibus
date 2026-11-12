using AmetisAPI.Core.Network;
using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    /// <summary>
    /// Tableau d'un arrêt : horaires théoriques du réseau complétés, quand Ametis publie la course en temps réel, par l'heure
    /// effective et l'écart. Rapprochement : même ligne et heures prévues à moins de <see cref="MatchToleranceSeconds"/> s
    /// (les deux GTFS n'ont pas d'identifiant de course commun). Un passage temps réel sans horaire théorique correspondant
    /// est affiché avec son heure prévue publiée.
    /// </summary>
    internal class GetStopBoardUseCase(
        ILogger<GetStopBoardUseCase> p_Logger,
        IUnitOfWork p_UnitOfWork,
        INetworkRepository p_Network,
        IRealtimeReferenceRepository p_References,
        IStopPassageRepository p_Passages,
        IRealtimeStore p_Store,
        IClock p_Clock)
        : ADbUseCase<GetStopBoardRequest, ValueResponse<StopBoard>>(p_Logger, p_UnitOfWork)
    {
        public const int MatchToleranceSeconds = 90;
        public const int RecentMinutes = 30;
        public const int RecentLimit = 10;
        private const int WindowMinutes = 180;
        private const int LookBackMinutes = 20;

        protected override async Task<ValueResponse<StopBoard>> HandleLogic(GetStopBoardRequest p_Request, CancellationToken p_CancellationToken)
        {
            DateTime v_Now = p_Clock.UtcNow;
            RealtimeSnapshot v_Snapshot = await p_Store.GetAsync(p_CancellationToken);
            int? v_FeedAge = v_Snapshot?.FeedTimestampUtc is { } v_Ts ? RealtimeRules.AgeSeconds(v_Now, v_Ts) : null;
            bool v_Stale = v_FeedAge is null || v_FeedAge > RealtimeRules.StaleAfterSeconds;

            return await WithDatabaseAsync(async () =>
            {
                StopDetail v_Stop = await p_Network.GetStopAsync(p_Request.StopId, p_CancellationToken);
                if (v_Stop is null)
                    return ValueResponse<StopBoard>.Error(DomainErrors.Data.NotFound, "Arrêt non trouvé");

                // On remonte un peu dans le temps : un bus prévu il y a 10 min mais en retard est encore à venir.
                List<Departure> v_Departures = await DepartureCalculator.ComputeAsync(p_Network, v_Stop,
                    NetworkTime.ToParis(v_Now.AddMinutes(-LookBackMinutes)), WindowMinutes + LookBackMinutes, p_CancellationToken);

                List<string> v_NetworkStopIds = [v_Stop.Stop.Id, .. v_Stop.Quays.Select(p_Q => p_Q.Id)];
                Dictionary<string, RealtimeStopMatch> v_StopMatches = (await p_References.GetStopMatchesForNetworkStopsAsync(v_NetworkStopIds, p_CancellationToken))
                    .ToDictionary(p_M => p_M.RealtimeStopId);
                IReadOnlyList<StopPassage> v_Passages = v_StopMatches.Count == 0 ? []
                    : await p_Passages.ListForStopsAsync([.. v_StopMatches.Keys], v_Now.AddMinutes(-RecentMinutes), v_Now.AddMinutes(WindowMinutes), p_CancellationToken);
                Dictionary<string, RealtimeLineMatch> v_Lines = (await p_References.GetLineMatchesAsync(p_CancellationToken)).ToDictionary(p_L => p_L.RealtimeRouteId);
                Dictionary<string, RealtimeTripInfo> v_Trips = (await p_References.GetTripsAsync(v_Passages.Select(p_P => p_P.TripId).Distinct().ToList(), p_CancellationToken))
                    .ToDictionary(p_T => p_T.TripId);

                List<StopBoardEntry> v_Upcoming = v_Departures.Select(p_D => new StopBoardEntry(p_D.Line, p_D.Destination, p_D.ScheduledTime, null, null, null, false, MatchConfidence.None, false)).ToList();
                HashSet<int> v_Passed = [];
                List<StopBoardEntry> v_Recent = [];

                foreach (StopPassage v_Passage in v_Passages.OrderBy(p_P => p_P.ScheduledUtc))
                {
                    RealtimeTripInfo v_Trip = v_Trips.GetValueOrDefault(v_Passage.TripId);
                    RealtimeLineMatch v_Line = (v_Passage.RouteId ?? v_Trip?.RouteId) is { } v_RouteId ? v_Lines.GetValueOrDefault(v_RouteId) : null;
                    MatchConfidence v_Confidence = RealtimeRules.Min(v_StopMatches[v_Passage.StopId].Confidence, v_Line?.Confidence ?? MatchConfidence.None);
                    DateTimeOffset v_Scheduled = NetworkTime.ToParis(v_Passage.ScheduledUtc);
                    int v_Index = v_Line is null ? -1 : v_Upcoming.FindIndex(p_E => !p_E.IsRealtime && p_E.Line.Id == v_Line.Line.Id
                        && Math.Abs((p_E.ScheduledTime - v_Scheduled).TotalSeconds) <= MatchToleranceSeconds);

                    if (RealtimeRules.IsPassed(v_Passage.ExpectedUtc, v_Now))
                    {
                        // Bus déjà passé : son horaire théorique ne doit plus apparaître dans les prochains passages.
                        if (v_Index >= 0)
                        {
                            v_Upcoming[v_Index] = v_Upcoming[v_Index] with { IsRealtime = true };
                            v_Passed.Add(v_Index);
                        }
                        v_Recent.Add(new StopBoardEntry(v_Line?.Line, v_Trip?.Headsign, v_Scheduled, NetworkTime.ToParis(v_Passage.ExpectedUtc), v_Passage.DelaySeconds,
                            v_Passage.TripId, true, v_Confidence, false));
                        continue;
                    }
                    if (v_Passage.IsLastStop)
                        continue; // terminus : arrivée sans montée possible, comme les horaires théoriques

                    StopBoardEntry v_Entry = new(v_Line?.Line, v_Trip?.Headsign, v_Scheduled, NetworkTime.ToParis(v_Passage.ExpectedUtc), v_Passage.DelaySeconds,
                        v_Passage.TripId, true, v_Confidence, v_Stale);
                    if (v_Index >= 0)
                    {
                        v_Upcoming[v_Index] = v_Entry with { Line = v_Upcoming[v_Index].Line, Destination = v_Upcoming[v_Index].Destination ?? v_Entry.Destination, ScheduledTime = v_Upcoming[v_Index].ScheduledTime };
                    }
                    else
                    {
                        v_Upcoming.Add(v_Entry);
                    }
                }

                DateTimeOffset v_Cutoff = NetworkTime.ToParis(v_Now.AddSeconds(-RealtimeRules.PastArrivalToleranceSeconds));
                return new ValueResponse<StopBoard>(new StopBoard(
                    v_Upcoming.Where((p_E, p_I) => !v_Passed.Contains(p_I)).Where(p_E => (p_E.ExpectedTime ?? p_E.ScheduledTime) >= v_Cutoff).OrderBy(p_E => p_E.ExpectedTime ?? p_E.ScheduledTime).Take(p_Request.Limit).ToList(),
                    v_Recent.OrderByDescending(p_E => p_E.ExpectedTime).Take(RecentLimit).ToList(),
                    v_FeedAge));
            }, p_CancellationToken);
        }
    }
}
