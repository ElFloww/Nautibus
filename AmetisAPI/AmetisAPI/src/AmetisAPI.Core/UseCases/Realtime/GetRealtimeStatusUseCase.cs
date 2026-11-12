using AmetisAPI.Core.Realtime;
using AmetisAPI.Core.Requests.Realtime;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Realtime;

namespace AmetisAPI.Core.UseCases.Realtime
{
    internal class GetRealtimeStatusUseCase(ILogger<GetRealtimeStatusUseCase> p_Logger, IRealtimeStore p_Store, IClock p_Clock)
        : AUseCase<GetRealtimeStatusRequest, ValueResponse<RealtimeStatus>>(p_Logger)
    {
        protected override async Task<ValueResponse<RealtimeStatus>> HandleLogic(GetRealtimeStatusRequest p_Request, CancellationToken p_CancellationToken)
        {
            return new(RealtimeStatusFactory.From(await p_Store.GetAsync(p_CancellationToken), p_Clock.UtcNow));
        }
    }
}
