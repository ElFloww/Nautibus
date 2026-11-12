using System.Reflection;
using AmetisAPI.API.Presenters.Crud;
using Autofac;
using Module = Autofac.Module;

namespace AmetisAPI.API
{
    /// <summary>
    /// Module for API
    /// </summary>
    internal class ApiModule : Module
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
            p_Builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("Presenter"))
                .InstancePerLifetimeScope();

            p_Builder.RegisterGeneric(typeof(GenericEntityListPresenter<>))
                    .AsSelf()
                    .InstancePerLifetimeScope();
        }
    }
}