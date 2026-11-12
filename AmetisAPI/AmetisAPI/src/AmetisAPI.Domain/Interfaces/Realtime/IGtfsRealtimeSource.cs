namespace AmetisAPI.Domain.Interfaces.Realtime
{
    public interface IGtfsRealtimeSource
    {
        Task<byte[]> FetchVehiclePositionsAsync(CancellationToken p_CancellationToken);

        Task<byte[]> FetchTripUpdatesAsync(CancellationToken p_CancellationToken);

        Task<byte[]> FetchAlertsAsync(CancellationToken p_CancellationToken);
    }
}
