using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Requests.Network
{
    public record GetLineRequest(string LineId) : IQuery<ValueResponse<LineDetail>>;
}
