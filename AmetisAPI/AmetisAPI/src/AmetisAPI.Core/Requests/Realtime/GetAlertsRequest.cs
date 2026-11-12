using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Requests.Realtime
{
    public record GetAlertsRequest : IQuery<ValueResponse<IReadOnlyList<ServiceAlert>>>;
}
