using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    /// <summary>
    /// Collecte des trois flux GTFS-RT. Chaque flux est traité séparément : un flux indisponible garde
    /// sa dernière valeur connue (qui vieillit et devient « obsolète ») et l'erreur est remontée dans l'état.
    /// </summary>
    internal class CollectRealtimeUseCase(
        ILogger<CollectRealtimeUseCase> p_Logger,
        IUnitOfWork p_UnitOfWork,
        IGtfsRealtimeSource p_Source,
        IGtfsRealtimeDecoder p_Decoder,
        IRealtimeStore p_Store,
        IVehicleHistoryRepository p_History,
        IStopPassageRepository p_Passages,
        IClock p_Clock)
        : ADbUseCase<CollectRealtimeRequest, ValueResponse<RealtimeStatus>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<RealtimeStatus>> HandleLogic(CollectRealtimeRequest p_Request, CancellationToken p_CancellationToken)
        {
            RealtimeSnapshot v_Previous = await p_Store.GetAsync(p_CancellationToken);
            List<string> v_Errors = [];

            RealtimeFeed v_Vehicles = await TryFetchAsync("vehicle-positions", p_Source.FetchVehiclePositionsAsync, v_Errors, p_CancellationToken);
            RealtimeFeed v_Trips = await TryFetchAsync("trip-updates", p_Source.FetchTripUpdatesAsync, v_Errors, p_CancellationToken);
            RealtimeFeed v_Alerts = await TryFetchAsync("alerts", p_Source.FetchAlertsAsync, v_Errors, p_CancellationToken);
            RealtimeFeed[] v_Fresh = [.. new[] { v_Vehicles, v_Trips, v_Alerts }.Where(p_F => p_F is not null)];

            RealtimeSnapshot v_Snapshot = new(
                v_Fresh.Select(p_F => p_F.TimestampUtc).Max() ?? v_Previous?.FeedTimestampUtc,
                p_Clock.UtcNow,
                v_Vehicles?.Vehicles ?? v_Previous?.Vehicles ?? [],
                v_Trips?.Predictions ?? v_Previous?.Predictions ?? [],
                v_Alerts?.Alerts ?? v_Previous?.Alerts ?? [],
                [.. v_Fresh.SelectMany(p_F => p_F.Anomalies)],
                v_Errors,
                v_Previous?.RouteStates);

            await p_Store.SaveAsync(v_Snapshot, p_CancellationToken);

            if (v_Trips is not null)
                await WithDatabaseAsync(() => p_Passages.UpsertAsync(StopPassageExtractor.Extract(v_Trips.Predictions), p_Clock.UtcNow, p_CancellationToken), p_CancellationToken);

            if (v_Vehicles is not null)
                await WithDatabaseAsync(() => p_History.AppendAsync([.. v_Vehicles.Vehicles], p_CancellationToken), p_CancellationToken);

            if (v_Snapshot.Anomalies.Count > 0)
                m_Logger.LogWarning("GTFS-RT : {Count} anomalies dans le flux, ex. {First}", v_Snapshot.Anomalies.Count, v_Snapshot.Anomalies[0]);

            return new(RealtimeStatusFactory.From(v_Snapshot, p_Clock.UtcNow));
        }

        private async Task<RealtimeFeed> TryFetchAsync(string p_Name, Func<CancellationToken, Task<byte[]>> p_Fetch, List<string> p_Errors, CancellationToken p_CancellationToken)
        {
            try
            {
                return p_Decoder.Decode(await p_Fetch(p_CancellationToken));
            }
            catch (Exception v_Ex) when (v_Ex is not OperationCanceledException)
            {
                m_Logger.LogWarning(v_Ex, "GTFS-RT : flux {Feed} indisponible", p_Name);
                p_Errors.Add($"Flux {p_Name} indisponible ou non décodable");
                return null;
            }
        }
    }
}
