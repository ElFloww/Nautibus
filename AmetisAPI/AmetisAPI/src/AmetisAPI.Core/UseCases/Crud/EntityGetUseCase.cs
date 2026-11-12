using AmetisAPI.Core.Requests.Crud;
using AmetisAPI.Core.Responses;
using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases.Crud
{
    internal class EntityGetUseCase<T>(IUnitOfWork p_UnitOfWork, ILogger<EntityGetUseCase<T>> p_Logger)
        : ADbUseCase<EntityGetRequest<T>, EntityResponse<T>>(p_Logger, p_UnitOfWork)
        where T : class, IBoEntity
    {
        protected override async Task<EntityResponse<T>> HandleLogic(EntityGetRequest<T> p_Request, CancellationToken p_CancellationToken)
        {
            try
            {
                await m_DatabaseProtection.WaitAsync(p_CancellationToken);

                T v_Entity = await m_UnitOfWork.GetCrudRepository<T>().GetEntityAsync(p_Request.EntityId, p_CancellationToken);

                if (v_Entity is null)
                {
                    return new EntityResponse<T>([
                        new ErrorDto(DomainErrors.Data.NotFound, $"{typeof(T).Name} non trouvé")
                    ]);
                }

                return new EntityResponse<T>(v_Entity);
            }
            finally
            {
                m_DatabaseProtection.Release();
            }
        }
    }
}