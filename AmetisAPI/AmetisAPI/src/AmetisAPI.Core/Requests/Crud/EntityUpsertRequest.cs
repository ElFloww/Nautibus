using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.Core.Requests.Crud
{
    public record EntityUpsertRequest<T>(T BoEntity) : IQuery<EntityResponse<T>>
       where T : class, IBoEntity;
}