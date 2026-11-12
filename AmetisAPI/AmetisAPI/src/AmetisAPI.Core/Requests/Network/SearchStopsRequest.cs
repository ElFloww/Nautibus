using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Requests.Network
{
    public record SearchStopsRequest(string Query, double? Latitude, double? Longitude, int RadiusMeters = 500, int Limit = 50) : IQuery<ValueResponse<IReadOnlyList<StopSummary>>>;
}
