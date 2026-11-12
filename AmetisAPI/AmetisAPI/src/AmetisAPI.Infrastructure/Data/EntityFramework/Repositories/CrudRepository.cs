using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Repositories;
using AmetisAPI.Infrastructure.Data.EntityFramework.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    internal class CrudRepository<T, U>(AppDbContext p_AppDbContext, IMapper p_Mapper, ILogger<CrudRepository<T, U>> p_Logger) : GenericRepository<U>(p_AppDbContext, p_Mapper, p_Logger), ICrudRepository<T>
        where T : class, IBoEntity
        where U : class, IIdEntity
    {
        internal virtual Func<IQueryable<U>, IIncludableQueryable<U, object>> Include() => null;

        private const string NotIdBoEntityMessage = "The entity isn't a IIdBoEntity";
        public async Task<T> GetEntityAsync(int p_EntityId, CancellationToken p_CancellationToken)
        {
            if (!typeof(T).IsAssignableTo(typeof(IIdBoEntity)))
                throw new ArgumentException(NotIdBoEntityMessage);

            Func<IQueryable<U>, IIncludableQueryable<U, object>> v_Include = Include();

            U v_Entity = await FindBy(p_E => p_E.Id == p_EntityId, p_Include: v_Include).SingleOrDefaultAsync(p_CancellationToken);

            return Mapper.Map<T>(v_Entity);
        }

        public async Task<IEnumerable<T>> ListEntitiesAsync(CancellationToken p_CancellationToken = default)
        {
            IEnumerable<U> v_Entities;

            v_Entities = await GetAllAsync(p_CancellationToken: p_CancellationToken);

            return Mapper.Map<IEnumerable<T>>(v_Entities);
        }

        public async Task<IMappingAddEntity<T, IEntity>> AddEntityAsync(T p_BoEntity, CancellationToken p_CancellationToken = default)
        {
            if (p_BoEntity is IIdBoEntity)
            {
                IMappingAddEntity<T, U> v_MappingAddEntity =
                    new MappingAddEntity<T, U>(Mapper, p_BoEntity);

                await Add(v_MappingAddEntity.DtoEntity, p_CancellationToken);
                return v_MappingAddEntity;
            }
            else
            {
                ArgumentException v_Exception = new(NotIdBoEntityMessage);
                p_Logger.LogError(v_Exception, NotIdBoEntityMessage);
                throw v_Exception;
            }
        }

        public async Task UpdateEntityAsync(T p_BoEntity, CancellationToken p_CancellationToken = default)
        {
            if (p_BoEntity is not IIdBoEntity v_BoEntity)
            {
                ArgumentException v_Exception = new(NotIdBoEntityMessage);
                p_Logger.LogError(v_Exception, NotIdBoEntityMessage);
                throw v_Exception;
            }

            U v_Entity = await Get(v_BoEntity.Id, p_CancellationToken);

            Mapper.Map<T, U>(p_BoEntity, v_Entity);

            await Update(v_Entity);
        }

        public async Task DeleteEntityAsync(int p_EntityId, CancellationToken p_CancellationToken)
        {
            if (!typeof(T).IsAssignableTo(typeof(IIdBoEntity)))
                throw new ArgumentException(NotIdBoEntityMessage);

            await Delete(p_EntityId, p_CancellationToken);
        }
    }
}