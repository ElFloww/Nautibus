using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmetisAPI.Core.Requests.Crud;
using AmetisAPI.Core.Responses;
using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases.Crud
{
    internal class EntityListUseCase<T>(IUnitOfWork p_UnitOfWork, ILogger<EntityListUseCase<T>> p_Logger)
        : ADbUseCase<EntityListRequest<T>, EntityListResponse<T>>(p_Logger, p_UnitOfWork)
        where T : class, IBoEntity
    {
        protected override async Task<EntityListResponse<T>> HandleLogic(EntityListRequest<T> p_Request, CancellationToken p_CancellationToken)
        {
            try
            {
                await m_DatabaseProtection.WaitAsync(p_CancellationToken);

                IEnumerable<T> v_List = await m_UnitOfWork.GetCrudRepository<T>().ListEntitiesAsync(p_CancellationToken);

                if (v_List is null)
                {
                    return new EntityListResponse<T>([
                        new ErrorDto(DomainErrors.UnknowError, $"{typeof(T).Name} non trouvé")
                    ]);
                }

                return new EntityListResponse<T>(v_List);
            }
            finally
            {
                m_DatabaseProtection.Release();
            }
        }
    }
}