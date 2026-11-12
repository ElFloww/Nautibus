using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Requests.Realtime
{
    public record GetVehiclesRequest : IQuery<ValueResponse<IReadOnlyList<VehicleView>>>;
}
