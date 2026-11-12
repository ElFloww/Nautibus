using Autofac.Core;

namespace AmetisAPI.Core.Tests
{
    public class CoreModuleTests
    {
        private static readonly string m_AutofacClass = $"{nameof(Autofac)}.";

        private readonly IList<Type> m_TypesWanted =
        [
        ];

        private IList<Type> m_TypesLoaded;

        [Fact]
        public void Should_Have_Register_Types()
        {
            ContainerBuilder v_ContainerBuilder = new();
            v_ContainerBuilder.RegisterModule(new CoreModule());

            IContainer v_Container = v_ContainerBuilder.Build();

            m_TypesLoaded = v_Container.ComponentRegistry.Registrations
                .SelectMany(p_X => p_X.Services)
                .Cast<IServiceWithType>()
                .Select(p_X => p_X.ServiceType)
                // Exclude types registered by AutoFac or IBaseRequest from MediatR
                .Where(p_C =>
                    !p_C.FullName.StartsWith(m_AutofacClass))
                .Distinct()
                .ToList();

            Check.That(m_TypesLoaded.OrderBy(p_C => p_C.FullName).ToList()).ContainsExactly(m_TypesWanted.OrderBy(p_C => p_C.FullName).ToList());
        }
    }
}