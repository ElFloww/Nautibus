using AmetisAPI.Core.UseCases.Crud;
using Autofac;

namespace AmetisAPI.Core
{
    /// <summary>
    /// Module for Core
    /// </summary>
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            CustomLoad(builder);
        }

        /// <summary>
        /// Custom load for all custom DI
        /// </summary>
        /// <param name="p_Builder"></param>
        private static void CustomLoad(ContainerBuilder p_Builder)
        {
            // Add here your services

            p_Builder.RegisterGeneric(typeof(EntityListUseCase<>))
                   .As(typeof(IQueryHandler<,>))
                   .InstancePerLifetimeScope();
        }
    }
}