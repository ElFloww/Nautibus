using AmetisAPI.Domain.Bo.Events;

namespace AmetisAPI.Core.Requests.Events
{
    public record ListNetworkEventsRequest(int Days = 7) : IQuery<ValueResponse<IReadOnlyList<NetworkEventView>>>;
}
