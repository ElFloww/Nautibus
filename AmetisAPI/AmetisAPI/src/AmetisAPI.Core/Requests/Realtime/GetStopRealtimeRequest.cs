using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Requests.Realtime
{
    public record GetStopRealtimeRequest(string StopId, int Limit = 20) : IQuery<ValueResponse<IReadOnlyList<RealtimeArrival>>>;
}
