using System.Linq.Expressions;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore.Query;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    public interface IGenericRepository<T> : IGenericRepository
       where T : class, IEntity
    {
        /// <summary>
        /// Check if an element exists for a condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A boolean</returns>
        Task<bool> Exists(Expression<Func<T, bool>> p_Predicate, CancellationToken p_CancellationToken = default);

        /// <summary>
        /// Gets the count based on a predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>The number of rows.</returns>
        Task<long> CountAsync(Expression<Func<T, bool>> p_Predicate = null, CancellationToken p_CancellationToken = default);

        /// <summary>
        /// Finds an entity with the given primary key value.
        /// </summary>
        /// <param name="p_Id">The value of the primary key.</param>
        /// <returns>The found entity or null.</returns>
        Task<T> Get(object p_Id, CancellationToken p_CancellationToken = default);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <param name="p_Include">Optional : A function to include navigation properties</param>
        /// <param name="p_OrderBy">Optional : A function to order elements.</param>
        /// <param name="p_Skip">Optional : Number of elements to skip</param>
        /// <param name="p_Take">Optional : Number of elements to take</param>
        /// <param name="p_DisableTracking">Optional : A boolean to disable entities changing tracking.</param>
        /// <returns>The all dataset.</returns>
        Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true, CancellationToken p_CancellationToken = default);

        /// <summary>
        /// Gets the entities based on a predicate, orderby and children inclusions.
        /// </summary>
        /// <param name="p_Predicate">A function to test each element for a condition.</param>
        /// <param name="p_OrderBy">Optional : A function to order elements.</param>
        /// <param name="p_Include">Optional : A function to include navigation properties</param>
        /// <param name="p_Skip">Optional : Number of elements to skip</param>
        /// <param name="p_Take">Optional : Number of elements to take</param>
        /// <param name="p_DisableTracking">Optional : A boolean to disable entities changing tracking.</param>
        /// <returns>A queryable list of elements satisfying the condition.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        IQueryable<T> FindBy(Expression<Func<T, bool>> p_Predicate, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true);

        /// <summary>
        /// Inserts a new entity.
        /// </summary>
        /// <param name="p_Entity">The entity to insert.</param>
        Task Add(T p_Entity, CancellationToken p_CancellationToken = default);

        /// <summary>
        /// Inserts a range of entities.
        /// </summary>
        /// <param name="p_Entities">The entities to insert.</param>
        Task Add(IEnumerable<T> p_Entities, CancellationToken p_CancellationToken = default);

        /// <summary>
        /// Update the specified entity.
        /// </summary>
        /// <param name="p_Entity">The entity.</param>
        Task Update(T p_Entity);

        /// <summary>
        /// Updates a range of entities.
        /// </summary>
        /// <param name="p_Entities">The entities to insert.</param>
        Task Update(IEnumerable<T> p_Entities);

        /// <summary>
        /// Deletes the entity by the specified primary key.
        /// </summary>
        /// <param name="p_Id">The primary key value.</param>
        Task Delete(object p_Id, CancellationToken p_CancellationToken = default);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="p_Entity">The entity to delete.</param>
        Task Delete(T p_Entity);

        /// <summary>
        /// Deletes Range entities.
        /// </summary>
        /// <param name="p_Entities">The entities list to delete.</param>
        Task Delete(IEnumerable<T> p_Entities);
    }
}