using Autofac.Builder;

namespace AmetisAPI.Core.Tests.Utilities
{
    internal static class ContainerBuilderTool
    {
        public static IComponentContext BuildContainer(params MockRegistrationEntry[] p_MockTuples)
        {
            ContainerBuilder v_ContainerBuilder = new();

            foreach (MockRegistrationEntry v_MockTuple in p_MockTuples)
            {
                IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> v_RegistrationBuilder = v_ContainerBuilder
                    .RegisterInstance(v_MockTuple.Mock.Object);
                if (!string.IsNullOrWhiteSpace(v_MockTuple.Key))
                {
                    v_RegistrationBuilder
                                    .Keyed(v_MockTuple.Key, v_MockTuple.Type);
                }
                else
                {
                    v_RegistrationBuilder
                                    .As(v_MockTuple.Type);
                }
            }

            return v_ContainerBuilder.Build();
        }
    }

    internal record MockRegistrationEntry(Type Type, Mock Mock)
    {
        public string Key { get; set; }
    }
}