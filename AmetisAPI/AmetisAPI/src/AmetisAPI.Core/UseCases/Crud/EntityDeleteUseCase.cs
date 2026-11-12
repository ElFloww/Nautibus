using AmetisAPI.Core.Requests.Crud;
using AmetisAPI.Core.Responses;
using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases.Crud
{
    internal class EntityDeleteUseCase<T>(IUnitOfWork p_UnitOfWork, ILogger<EntityDeleteUseCase<T>> p_Logger)
        : ADbUseCase<EntityDeleteRequest<T>, DeleteListResponse>(p_Logger, p_UnitOfWork)
        where T : class, IBoEntity
    {
        protected override async Task<DeleteListResponse> HandleLogic(EntityDeleteRequest<T> p_Request, CancellationToken p_CancellationToken)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(p_Request);

                await m_DatabaseProtection.WaitAsync(p_CancellationToken);
                Dictionary<int, bool> v_Result = [];

                foreach (int v_Id in p_Request.EntitiesId)
                {
                    try
                    {
                        await m_UnitOfWork.GetCrudRepository<T>().DeleteEntityAsync(v_Id, p_CancellationToken);
                        m_UnitOfWork.Save();

                        v_Result.Add(v_Id, true);
                    }
                    catch (Exception v_Ex)
                    {
                        m_Logger.LogError(v_Ex, "An error occured while trying to delete an entity");
                        v_Result.Add(v_Id, false);
                    }
                }

                return new DeleteListResponse(v_Result);
            }
            finally
            {
                m_DatabaseProtection.Release();
            }
        }
    }
}