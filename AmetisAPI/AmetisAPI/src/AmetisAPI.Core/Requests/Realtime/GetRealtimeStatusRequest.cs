using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Requests.Realtime
{
    public record GetRealtimeStatusRequest : IQuery<ValueResponse<RealtimeStatus>>;
}
