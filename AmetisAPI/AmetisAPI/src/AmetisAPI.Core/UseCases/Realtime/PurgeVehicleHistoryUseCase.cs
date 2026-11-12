using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    /// <summary>Rétention de 30 jours de l'historique des positions de véhicules.</summary>
    internal class PurgeVehicleHistoryUseCase(ILogger<PurgeVehicleHistoryUseCase> p_Logger, IUnitOfWork p_UnitOfWork, IVehicleHistoryRepository p_History, Domain.Interfaces.Events.INetworkEventRepository p_Events, IStopPassageRepository p_Passages, IClock p_Clock)
        : ADbUseCase<PurgeVehicleHistoryRequest, ValueResponse<int>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<ValueResponse<int>> HandleLogic(PurgeVehicleHistoryRequest p_Request, CancellationToken p_CancellationToken)
        {
            DateTime v_Limit = p_Clock.UtcNow.AddDays(-RealtimeRules.HistoryRetentionDays);
            int v_Deleted = await WithDatabaseAsync(() => p_History.PurgeOlderThanAsync(v_Limit, p_CancellationToken), p_CancellationToken);
            int v_Passages = await WithDatabaseAsync(() => p_Passages.PurgeOlderThanAsync(v_Limit, p_CancellationToken), p_CancellationToken);
            m_Logger.LogInformation("Passages antérieurs au {Limit:o} supprimés : {Count}", v_Limit, v_Passages);
            int v_Events = await WithDatabaseAsync(() => p_Events.PurgeClosedBeforeAsync(v_Limit, p_CancellationToken), p_CancellationToken);
            m_Logger.LogInformation("Évènements réseau clos avant le {Limit:o} supprimés : {Count}", v_Limit, v_Events);
            m_Logger.LogInformation("Historique des positions : {Count} positions antérieures au {Limit:o} supprimées", v_Deleted, v_Limit);
            return new(v_Deleted);
        }
    }
}
