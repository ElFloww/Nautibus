namespace AmetisAPI.API.Tests
{
    public class ApiModuleTests
    {
        private static readonly string m_AutofacClass = $"{nameof(Autofac)}.";

        private readonly IList<Type> m_TypesWanted =
        [
            // Common presenters
            typeof(EmptyPresenter),
            typeof(ValidationPresenter),
            typeof(DownloadFilePresenter),
            typeof(DeleteListPresenter),
            typeof(IntPresenter)
        ];

        private IList<Type> m_TypesLoaded;

        [Fact]
        public void Should_Have_Register_Types()
        {
            ContainerBuilder v_ContainerBuilder = new();
            v_ContainerBuilder.RegisterModule(new ApiModule());

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

            Check.That(m_TypesLoaded.OrderBy(p_C => p_C.FullName)).ContainsExactly(m_TypesWanted.OrderBy(p_C => p_C.FullName));
        }
    }
}