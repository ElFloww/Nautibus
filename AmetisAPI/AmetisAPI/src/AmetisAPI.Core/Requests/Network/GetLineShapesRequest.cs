using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Requests.Network
{
    public record GetLineShapesRequest(string LineId) : IQuery<ValueResponse<IReadOnlyList<LineShape>>>;
}
