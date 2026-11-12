using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework
{
    /// <summary>
    /// DbContext factory => get EF core DbContext for tests
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class DbContextFactory<T> where T : DbContext
    {
        /// <summary>
        /// Get DbContext of type <typeparamref name="T"/> from a connectionstring
        /// </summary>
        /// <param name="p_ConnectionString">Connection string to use</param>
        /// <returns></returns>
        internal static T InitContext(string p_ConnectionString)
        {
            if (string.IsNullOrWhiteSpace(p_ConnectionString))
                throw new ArgumentNullException(nameof(p_ConnectionString), "Connectionstring is need to create a context. Cannot be null or empty");

            DbContextOptionsBuilder<T> v_SqlOptions = new DbContextOptionsBuilder<T>().UseSqlServer(p_ConnectionString, p_Sql => p_Sql.UseNetTopologySuite()).EnableSensitiveDataLogging();

#pragma warning disable S125 // Sections of code should not be commented out => I keep this code in case I need a log for EF
            //v_SqlOptions.EnableSensitiveDataLogging();
            //v_SqlOptions.LogTo(log =>
            //{
            //    Console.WriteLine(log);
            //}, Microsoft.Extensions.Logging.LogLevel.Debug);
            //DbContextOptions<T> v_DbContextOptions = new DbContextOptionsBuilder<T>().UseSqlServer(p_ConnectionString).Options; //SQL Server Provider

            return (T)Activator.CreateInstance(typeof(T), v_SqlOptions.Options); //create new instance of DbContext of type T
#pragma warning restore S125 // Sections of code should not be commented out
        }
    }
}