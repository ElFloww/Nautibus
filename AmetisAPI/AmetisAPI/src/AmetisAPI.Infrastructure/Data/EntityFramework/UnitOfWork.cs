using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Repositories;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AmetisAPI.Infrastructure.Data.EntityFramework
{
    internal class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        private readonly TContext m_DbContext;
        private readonly IMapper m_Mapper;
        private readonly ILoggerFactory m_LoggerFactory;
        private readonly IComponentContext m_ContainerBuilder;

        public UnitOfWork(TContext p_DbContext, IMapper p_Mapper, ILoggerFactory p_LoggerFactory, IComponentContext p_ComponentBuilder)
        {
            m_DbContext = p_DbContext ?? throw new ArgumentNullException(nameof(p_DbContext));
            m_Mapper = p_Mapper ?? throw new ArgumentNullException(nameof(p_Mapper));
            m_LoggerFactory = p_LoggerFactory ?? throw new ArgumentNullException(nameof(p_LoggerFactory));
            m_ContainerBuilder = p_ComponentBuilder ?? throw new ArgumentNullException(nameof(p_ComponentBuilder));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Save()
        {
            int v_Result = m_DbContext.SaveChanges();
            m_DbContext.ChangeTracker?.Clear();
            return v_Result;
        }

        public void BeginTransaction() => m_DbContext.Database.BeginTransaction();

        public void CommitTransaction() => m_DbContext.Database.CommitTransaction();

        public void RollbackTransaction() => m_DbContext.Database.RollbackTransaction();

        public ICrudRepository<T> GetCrudRepository<T>()
            where T : class, IBoEntity
        {
            return m_ContainerBuilder.Resolve<ICrudRepository<T>>();
        }

        public T ExecutionStrategy<T>(Func<T> p_Func)
        {
            IExecutionStrategy v_Strategy = m_DbContext.Database.CreateExecutionStrategy();
            return v_Strategy.Execute(() => p_Func());
        }

        protected virtual void Dispose(bool p_Disposing)
        {
            if (p_Disposing)
                m_DbContext.Dispose();
        }

        private Lazy<TInterface> CreateLazy<TInterface, TImplem>()
            where TInterface : IGenericRepository
            where TImplem : class, IGenericRepository
        {
            return new Lazy<TInterface>(() => (TInterface)Activator.CreateInstance(typeof(TImplem), m_DbContext, m_Mapper, m_LoggerFactory.CreateLogger<TImplem>()));
        }
    }
}