using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Repositories;

namespace AmetisAPI.Domain.Interfaces.Data.EntityFramework
{
    public interface IUnitOfWork : IDisposable
    {
        int Save();

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        T ExecutionStrategy<T>(Func<T> p_Func);

        ICrudRepository<T> GetCrudRepository<T>() where T : class, IBoEntity;
    }
}