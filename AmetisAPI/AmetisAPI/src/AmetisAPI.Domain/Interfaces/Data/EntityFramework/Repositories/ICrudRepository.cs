using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Domain.Interfaces.Data.EntityFramework.Repositories
{
    public interface ICrudRepository<T>
        where T : class, IBoEntity
    {
        Task<T> GetEntityAsync(int p_EntityId, CancellationToken p_CancellationToken);

        Task<IEnumerable<T>> ListEntitiesAsync(CancellationToken p_CancellationToken = default);

        Task<IMappingAddEntity<T, IEntity>> AddEntityAsync(T p_BoEntity, CancellationToken p_CancellationToken = default);

        Task UpdateEntityAsync(T p_BoEntity, CancellationToken p_CancellationToken = default);

        Task DeleteEntityAsync(int p_EntityId, CancellationToken p_CancellationToken);
    }
}