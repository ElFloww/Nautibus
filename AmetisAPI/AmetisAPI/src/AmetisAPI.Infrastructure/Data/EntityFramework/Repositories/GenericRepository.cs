using System.Linq.Expressions;
using System.Reflection;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using AmetisAPI.Infrastructure.Data.EntityFramework.Mapping;
using AmetisAPI.Infrastructure.Tools;
using LinqKit;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Generic repository for each entity of the DbContext
    /// </summary>
    /// <typeparam name="T">Entity of type <see cref="IEntity"/></typeparam>
    internal abstract class GenericRepository<T> : AMappingClass, IGenericRepository<T> where T : class, IEntity
    {
        protected readonly DbContext m_DbContext;
        protected readonly DbSet<T> m_DbSet;
        private const string EntityCannotBeNull = "Entity cannot be null";
        private const string IdCannotBeNull = "Id cannot be null";
        private const string EntityCannotBeNullOrEmpty = "Entity cannot be null or empty";
        private const string DbcontextCannotBeNull = "DbContext cannot be null";

        protected GenericRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger p_Logger) : base(p_Mapper)
        {
            m_DbContext = Guard.Against.Null(p_DbContext, message: DbcontextCannotBeNull);
            m_DbSet = m_DbContext.Set<T>();
            Logger = p_Logger;
        }

        protected ILogger Logger { get; set; }

        /// <summary>
        /// Get the current <see cref="DbContext"/>
        /// </summary>
        public DbContext DbContext => m_DbContext;

        public Task Add(T p_Entity, CancellationToken p_CancellationToken = default)
        {
            Guard.Against.Null(p_Entity, message: EntityCannotBeNull);

            return AddInternalAsync(p_Entity, p_CancellationToken);
        }

        public Task Add(IEnumerable<T> p_Entities, CancellationToken p_CancellationToken = default)
        {
            if (p_Entities?.Any() != true)
                throw new ArgumentNullException(nameof(p_Entities), EntityCannotBeNullOrEmpty);

            return AddInternalAsync(p_Entities, p_CancellationToken);
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> p_Predicate = null, CancellationToken p_CancellationToken = default)
        {
            return p_Predicate == null ? await m_DbSet.LongCountAsync(cancellationToken: p_CancellationToken) : await m_DbSet.LongCountAsync(p_Predicate, cancellationToken: p_CancellationToken);
        }

        public Task Delete(object p_Id, CancellationToken p_CancellationToken = default)
        {
            if (p_Id == null)
                throw new ArgumentNullException(nameof(p_Id), IdCannotBeNull);

            return DeleteInternalAsync(p_Id, p_CancellationToken);
        }

        public Task Delete(T p_Entity)
        {
            if (p_Entity == null)
                throw new ArgumentNullException(nameof(p_Entity), EntityCannotBeNull);

            return DeleteInternal(p_Entity);
        }

        public Task Delete(IEnumerable<T> p_Entities)
        {
            if (p_Entities == null)
                throw new ArgumentNullException(nameof(p_Entities), EntityCannotBeNull);
            return DeleteInternal(p_Entities);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool p_Disposing)
        {
            // Cleanup
        }

        public Task<bool> Exists(Expression<Func<T, bool>> p_Predicate, CancellationToken p_CancellationToken = default)
        {
            Guard.Against.Null(p_Predicate, message: "Predicate cannot be null or empty");

            return ExistsInternalAsync(p_Predicate, p_CancellationToken);
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> p_Predicate, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true)
        {
            IQueryable<T> v_Query = m_DbSet;
            if (p_DisableTracking)
                v_Query = v_Query.AsNoTracking();

            if (p_Include != null)
                v_Query = p_Include(v_Query);

            if (p_Predicate != null)
                v_Query = v_Query.Where(p_Predicate);

            if (p_OrderBy != null)
                v_Query = p_OrderBy(v_Query);

            if (p_Skip.HasValue)
                v_Query = v_Query.Skip(p_Skip.Value);

            if (p_Take.HasValue)
                v_Query = v_Query.Take(p_Take.Value);

            return v_Query;
        }

        public async Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true, CancellationToken p_CancellationToken = default)
        {
            return await GetAllInternal(p_Include, p_OrderBy, p_Skip, p_Take, p_DisableTracking).ToListAsync(cancellationToken: p_CancellationToken);
        }

        public Task<T> Get(object p_Id, CancellationToken p_CancellationToken = default)
        {
            Guard.Against.Null(p_Id, message: IdCannotBeNull);

            return GetInternalAsync(p_Id, p_CancellationToken);
        }

        public Task Update(T p_Entity)
        {
            Guard.Against.Null(p_Entity, message: EntityCannotBeNull);

            return UpdateInternal(p_Entity);
        }

        public Task Update(IEnumerable<T> p_Entities)
        {
            if (p_Entities?.Any() != true)
                throw new ArgumentNullException(nameof(p_Entities), EntityCannotBeNullOrEmpty);

            return UpdateInternal(p_Entities);
        }

        /// <summary>
        /// Map BOEntity to EF Core Entity (prevent tracking exception)
        /// </summary>
        /// <typeparam name="TT">BOEntity type</typeparam>
        /// <param name="p_BoEntity">instance of BOEntity</param>
        /// <returns>instance of EF Core Entity</returns>
        internal T MapEntity<TT>(TT p_BoEntity) where TT : class
        {
            Guard.Against.Null(p_BoEntity);

            // AutoMapper previously provided a way to introspect the mapping configuration
            // and only map when the PK wasn't the default value.
            // With Mapster we keep the behavior simple: if a tracked local entity with same PK exists,
            // map onto it; otherwise map to a new instance.

            IReadOnlyList<IProperty> v_KeyProps = m_DbContext.Model
                .FindEntityType(typeof(T))
                ?.FindPrimaryKey()
                ?.Properties;

            if (v_KeyProps is { Count: > 0 })
            {
                // build expression TEntity => TEntity.PK == bo.PK && ...
                Expression<Func<T, bool>> v_Expression = PredicateBuilder.New<T>(true);

                foreach (IProperty v_KeyProp in v_KeyProps)
                {
                    PropertyInfo v_BoProperty = typeof(TT).GetProperty(v_KeyProp.Name);
                    object v_BoKeyValue = v_BoProperty?.GetValue(p_BoEntity);

                    // If BO PK is default, it is an unsaved object: map to a new instance.
                    if (v_BoProperty == null || v_BoKeyValue?.Equals(v_BoProperty.PropertyType.GetDefaultValue()) == true)
                        return Mapper.Map<T>(p_BoEntity);

                    PropertyInfo v_EntityProperty = typeof(T).GetProperty(v_KeyProp.Name);
                    if (v_EntityProperty == null)
                        return Mapper.Map<T>(p_BoEntity);

                    v_Expression = v_Expression.And(p_Entity => v_EntityProperty.GetValue(p_Entity).Equals(v_BoKeyValue));
                }

                T v_LocalEntity = m_DbSet.Local.SingleOrDefault(v_Expression.Compile());
                if (v_LocalEntity != null)
                {
                    m_DbContext.Entry(v_LocalEntity).State = EntityState.Detached;
                    return Mapper.Map(p_BoEntity, v_LocalEntity);
                }
            }

            return Mapper.Map<T>(p_BoEntity);
        }

        private async Task AddInternalAsync(T p_Entity, CancellationToken p_CancellationToken)
        {
            await m_DbSet.AddAsync(p_Entity, p_CancellationToken);
        }

        private async Task AddInternalAsync(IEnumerable<T> p_Entities, CancellationToken p_CancellationToken)
        {
            await m_DbSet.AddRangeAsync(p_Entities, p_CancellationToken);
        }

        private async Task DeleteInternalAsync(object p_Id, CancellationToken p_CancellationToken)
        {
            T v_Entity = await m_DbSet.FindAsync([p_Id], cancellationToken: p_CancellationToken);
            await Delete(v_Entity);
        }

        private Task DeleteInternal(T p_Entity)
        {
            if (m_DbContext.Entry(p_Entity).State == EntityState.Detached)
            {
                m_DbSet.Attach(p_Entity);
            }
            m_DbSet.Remove(p_Entity);
            return Task.CompletedTask;
        }

        private Task DeleteInternal(IEnumerable<T> p_Entities)
        {
            m_DbSet.RemoveRange(p_Entities);
            return Task.CompletedTask;
        }

        private async Task<bool> ExistsInternalAsync(Expression<Func<T, bool>> p_Predicate, CancellationToken p_CancellationToken)
        {
            return await m_DbSet.AnyAsync(p_Predicate, cancellationToken: p_CancellationToken);
        }

        private IQueryable<T> GetAllInternal(Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true)
        {
            return FindBy(null, p_OrderBy, p_Include, p_Skip, p_Take, p_DisableTracking);
        }

        private async Task<T> GetInternalAsync(object p_Id, CancellationToken p_CancellationToken)
        {
            return await m_DbSet.FindAsync([p_Id], cancellationToken: p_CancellationToken);
        }

        private Task UpdateInternal(T p_Entity)
        {
            if (((int)m_DbContext.Entry(p_Entity).State) < 2)
                m_DbContext.Entry(p_Entity).State = EntityState.Modified;
            m_DbSet.Update(p_Entity);
            return Task.CompletedTask;
        }

        private Task UpdateInternal(IEnumerable<T> p_Entities)
        {
            m_DbSet.UpdateRange(p_Entities);
            return Task.CompletedTask;
        }
    }
}