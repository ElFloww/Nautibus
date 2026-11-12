using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Requests.Network
{
    public record GetStopRequest(string StopId) : IQuery<ValueResponse<StopDetail>>;
}
