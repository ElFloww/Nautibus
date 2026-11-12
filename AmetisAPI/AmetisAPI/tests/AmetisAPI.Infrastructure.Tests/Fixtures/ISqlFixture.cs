using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Tests.Data.EntityFramework;

namespace AmetisAPI.Infrastructure.Tests.Fixtures
{
    public interface ISqlFixture : IDisposable
    {
        internal const string Collection = "SqlServer-Collection";
        public string ConnectionString { get; }
        public TestContext TestContext { get; }
        public AppDbContext AppDbContext { get; }
    }
}