using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Core.Requests.Gtfs
{
    public record GetDataHealthRequest : IQuery<ValueResponse<DataHealth>>;
}
