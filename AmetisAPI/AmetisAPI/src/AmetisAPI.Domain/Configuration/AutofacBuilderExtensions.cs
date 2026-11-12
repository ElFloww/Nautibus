using System.Configuration;
using Autofac;
using Autofac.Builder;
using Microsoft.Extensions.Configuration;

namespace AmetisAPI.Domain.Configuration
{
    public static class AutofacBuilderExtensions
    {
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterConfiguration<T>(this ContainerBuilder p_Builder) where T : IServiceConfiguration, new()
        {
            return p_Builder.Register(p_Context =>
            {
                T v_Config = new();
                IConfiguration v_Configuration = p_Context.Resolve<IConfiguration>();
                string v_SectionName = v_Config.GetSectionName();
                IConfigurationSection v_ConfigurationSection =
                    v_Configuration.GetSection(v_SectionName);
                if (!v_ConfigurationSection.Exists())
                    throw new ConfigurationErrorsException($"{v_SectionName} section is mandatory");
                v_ConfigurationSection
                    .Bind(v_Config, p_Options => p_Options.BindNonPublicProperties = true);

                return v_Config;
            }).SingleInstance();
        }
    }
}