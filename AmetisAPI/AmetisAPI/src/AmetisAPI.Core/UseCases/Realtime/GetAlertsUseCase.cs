using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    /// <summary>Perturbations en cours (fenêtre d'application non échue).</summary>
    internal class GetAlertsUseCase(ILogger<GetAlertsUseCase> p_Logger, IRealtimeStore p_Store, IClock p_Clock)
        : AUseCase<GetAlertsRequest, ValueResponse<IReadOnlyList<ServiceAlert>>>(p_Logger)
    {
        protected override async Task<ValueResponse<IReadOnlyList<ServiceAlert>>> HandleLogic(GetAlertsRequest p_Request, CancellationToken p_CancellationToken)
        {
            RealtimeSnapshot v_Snapshot = await p_Store.GetAsync(p_CancellationToken);
            DateTime v_Now = p_Clock.UtcNow;
            return new((v_Snapshot?.Alerts ?? []).Where(p_A => (p_A.StartUtc ?? DateTime.MinValue) <= v_Now && v_Now <= (p_A.EndUtc ?? DateTime.MaxValue)).ToList());
        }
    }
}
