using AmetisAPI.Core.Requests.Crud;
using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Enums.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Core.UseCases.Crud
{
    internal class EntityUpsertUseCase<T>(IUnitOfWork p_UnitOfWork, ILogger<EntityUpsertUseCase<T>> p_Logger)
            : ADbUseCase<EntityUpsertRequest<T>, EntityResponse<T>>(p_Logger, p_UnitOfWork)
            where T : class, IIdBoEntity
    {
        protected UpsertMode m_UpsertMode;
        protected T m_Bo;

        protected override async Task<EntityResponse<T>> HandleLogic(EntityUpsertRequest<T> p_Request, CancellationToken p_CancellationToken)
        {
            CheckRequest(p_Request);
            try
            {
                await m_DatabaseProtection.WaitAsync(p_CancellationToken);

                m_Bo = p_Request.BoEntity;

                int v_EntityId;
                SaveInnerEntity(p_Request);
                if (p_Request.BoEntity.Id.HasValue)
                {
                    m_UpsertMode = UpsertMode.Update;
                    await m_UnitOfWork.GetCrudRepository<T>().UpdateEntityAsync(p_Request.BoEntity, p_CancellationToken);
                    v_EntityId = p_Request.BoEntity.Id!.Value;
                }
                else
                {
                    m_UpsertMode = UpsertMode.Insert;
                    IMappingAddEntity<T, IEntity> v_Inserted = await m_UnitOfWork.GetCrudRepository<T>().AddEntityAsync(p_Request.BoEntity, p_CancellationToken);
                    m_UnitOfWork.Save();
                    m_Bo = v_Inserted.MapBoEntity;
                    v_EntityId = v_Inserted.MapBoEntity.Id!.Value;
                }

                await CallInnerEntityRepoAsync(v_EntityId, p_CancellationToken);
                m_UnitOfWork.Save();
            }
            finally
            {
                m_DatabaseProtection.Release();
            }

            return new EntityResponse<T>(m_Bo);
        }

        protected virtual void CheckRequest(EntityUpsertRequest<T> p_Request)
        {
            ArgumentNullException.ThrowIfNull(p_Request);
            ArgumentNullException.ThrowIfNull(p_Request.BoEntity);
        }

        protected virtual void SaveInnerEntity(EntityUpsertRequest<T> p_Request)
        { }

        protected virtual Task CallInnerEntityRepoAsync(int p_EntityId, CancellationToken p_CancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual void ReaffectInnerEntityAsync(ref T p_Bo)
        { }
    }
}