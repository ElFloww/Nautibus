using AmetisAPI.Infrastructure.Data.EntityFramework.Mapping;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Mapping
{
    public class EntityFrameworkProfileTests
    {
        [Fact]
        public void ValidateInfraProfile()
        {
            Check.ThatCode(() =>
            {
                var v_Config = new TypeAdapterConfig();

                new EntityFrameworkProfile().Register(v_Config);

                v_Config.Compile();
            }
            ).DoesNotThrow();
        }


    }
}