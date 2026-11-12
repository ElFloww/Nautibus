using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Tests.Data.EntityFramework;
using Microsoft.SqlServer.Dac;

namespace AmetisAPI.Infrastructure.Tests.Fixtures
{
    public abstract class ASqlServerFixture(string p_ConnectionString) : ISqlFixture
    {
        #region Constants

        protected const string Tudbname = "1E1F_TU";
        protected const string Dacpacpathkey = "DACPACPATH";

        #endregion Constants

        public string ConnectionString { get; } = p_ConnectionString;
        public TestContext TestContext { get; } = DbContextFactory<TestContext>.InitContext(p_ConnectionString);
        public AppDbContext AppDbContext { get; } = DbContextFactory<AppDbContext>.InitContext(p_ConnectionString);

        /// <summary>
        /// Get dacpac file path from environment variables
        /// </summary>
        /// <returns></returns>
        protected string GetDacPacPath()
        {
            string v_DacPacFilePath = Environment.GetEnvironmentVariable(Dacpacpathkey);

            if (string.IsNullOrWhiteSpace(v_DacPacFilePath))
            {
                Dispose(true);
                throw new FileNotFoundException("Path of the dacpac is null or empty");
            }

            string v_Path = Path.GetFullPath(v_DacPacFilePath);
            if (!File.Exists(v_Path))
            {
                Dispose(true);
                throw new FileNotFoundException($"File {v_Path} doesn't exist");
            }

            return v_Path;
        }

        /// <summary>
        /// Deploy dacpac to SQL Server on the docker container
        /// </summary>
        /// <param name="p_Path"></param>
        protected void DeployDacPac(string p_Path)
        {
            using DacPackage v_DacPac = DacPackage.Load(p_Path);
            DacServices v_DbInstance = new(ConnectionString);
            DacDeployOptions v_DacOptions = new() { AllowIncompatiblePlatform = true, VerifyDeployment = true };
            v_DbInstance.Deploy(v_DacPac, Tudbname, upgradeExisting: true, options: v_DacOptions);
        }

        /// <summary>
        /// Dispose the fixture
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool p_Disposing);
    }
}