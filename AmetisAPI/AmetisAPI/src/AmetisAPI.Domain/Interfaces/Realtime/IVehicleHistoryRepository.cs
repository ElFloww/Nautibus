using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Interfaces.Realtime
{
    public interface IVehicleHistoryRepository
    {
        /// <summary>Ajoute les positions ; une position déjà stockée (même véhicule, même horodatage) n'est pas dupliquée.</summary>
        Task<int> AppendAsync(IReadOnlyCollection<VehiclePosition> p_Positions, CancellationToken p_CancellationToken);

        /// <summary>Positions enregistrées d'une course dans l'intervalle, dans l'ordre chronologique ([longitude, latitude]).</summary>
        Task<IReadOnlyList<double[]>> GetTripTraceAsync(string p_TripId, DateTime p_FromUtc, DateTime p_ToUtc, CancellationToken p_CancellationToken);

        Task<int> PurgeOlderThanAsync(DateTime p_LimitUtc, CancellationToken p_CancellationToken);
    }
}
