using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AmetisAPI.API.Swagger
{
    /// <summary>
    /// Swagger api versioning configuration
    /// </summary>
    /// <remarks>
    /// ctor
    /// </remarks>
    /// <param name="p_Provider"></param>
    public class ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider p_Provider)
            : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider m_Provider = p_Provider;

        /// <summary>
        /// Configure each API discovered for Swagger Documentation
        /// </summary>
        /// <param name="options"></param>
        public void Configure(SwaggerGenOptions options)
        {
            // add swagger document for every API version discovered
            foreach (ApiVersionDescription v_Description in m_Provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    v_Description.GroupName,
                    CreateVersionInfo(v_Description));
            }
        }

        /// <summary>
        /// Configure Swagger Options. Inherited from the Interface
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void Configure(string name, SwaggerGenOptions options)
        {
            Configure(options);
        }

        /// <summary>
        /// Create information about the version of the API
        /// </summary>
        /// <param name="p_Desc"></param>
        /// <returns>Information about the API</returns>
        private static OpenApiInfo CreateVersionInfo(
            ApiVersionDescription p_Desc)
        {
            OpenApiInfo v_Info = new()
            {
                Title = typeof(ConfigureSwaggerOptions).Assembly.GetName().Name,
                Version = p_Desc.ApiVersion.ToString()
            };

            if (p_Desc.IsDeprecated)
            {
                v_Info.Description += " This API version has been deprecated. Please use one of the new APIs available from the explorer.";
            }

            return v_Info;
        }
    }
}