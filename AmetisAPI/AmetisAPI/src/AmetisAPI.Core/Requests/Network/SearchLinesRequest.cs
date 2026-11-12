using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Requests.Network
{
    public record SearchLinesRequest(string Query) : IQuery<ValueResponse<IReadOnlyList<LineSummary>>>;
}
