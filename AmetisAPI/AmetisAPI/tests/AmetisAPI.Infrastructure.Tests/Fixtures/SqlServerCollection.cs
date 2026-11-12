namespace AmetisAPI.Infrastructure.Tests.Fixtures
{
    [CollectionDefinition(ISqlFixture.Collection, DisableParallelization = true)]
    public class SqlServerCollection : ICollectionFixture<LocalDbFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}