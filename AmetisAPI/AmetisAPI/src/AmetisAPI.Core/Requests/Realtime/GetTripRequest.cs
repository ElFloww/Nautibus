using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Requests.Realtime
{
    public record GetTripRequest(string TripId) : IQuery<ValueResponse<TripDetail>>;
}
