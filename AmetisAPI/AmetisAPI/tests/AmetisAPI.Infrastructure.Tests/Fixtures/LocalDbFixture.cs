using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac;
using Polly;

namespace AmetisAPI.Infrastructure.Tests.Fixtures
{
    public class LocalDbFixture : ASqlServerFixture
    {
        private static readonly string m_ConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={Tudbname};Integrated Security=True;MultipleActiveResultSets=True;";
        private static readonly Policy m_WaitDeployDacPac = Policy.Handle<DacServicesException>().Or<AggregateException>().WaitAndRetry(5, p_Retry => TimeSpan.FromSeconds(1));

        public LocalDbFixture() : base(m_ConnectionString)
        {
            //get settings from launchsettings
            using LaunchSettingsFixture v_LaunchSettingsFixture = new();
            string v_DacPacPath = GetDacPacPath(); //in a first time we check if we have a dacpac file which existing
            m_WaitDeployDacPac.Execute(() => DeployDacPac(v_DacPacPath)); //wait deploy dacpac
        }

        protected override void Dispose(bool p_Disposing)
        {
            if (AppDbContext != null)
            {
                AppDbContext.Database.EnsureDeleted();
                AppDbContext.Database.CloseConnection();
                AppDbContext.Dispose();
            }

            if (TestContext != null)
            {
                TestContext.Database.EnsureDeleted();
                TestContext.Database.CloseConnection();
                TestContext.Dispose();
            }
        }
    }
}