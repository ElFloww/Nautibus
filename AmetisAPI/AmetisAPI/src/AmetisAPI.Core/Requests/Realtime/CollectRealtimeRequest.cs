using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Requests.Realtime
{
    public record CollectRealtimeRequest : IQuery<ValueResponse<RealtimeStatus>>;
}
