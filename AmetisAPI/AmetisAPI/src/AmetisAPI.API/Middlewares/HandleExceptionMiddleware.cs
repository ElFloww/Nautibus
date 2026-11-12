using System.Net.Mime;

namespace AmetisAPI.API.Middlewares
{
    /// <summary>
    /// Render an uncatch exception into an HTTP response.
    /// </summary>
    internal sealed class HandleExceptionMiddleware(RequestDelegate p_Next, ILogger<HandleExceptionMiddleware> p_Logger)
    {
        private readonly ILogger m_Logger = p_Logger;

        public async Task InvokeAsync(HttpContext p_Context)
        {
            try
            {
                await p_Next(p_Context);
            }
            catch (Exception v_Ex)
            {
                try
                {
                    m_Logger.LogError(v_Ex, "An uncatched error occurred");

                    if (!p_Context.Response.HasStarted)
                    {
                        p_Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        p_Context.Response.ContentType = MediaTypeNames.Application.ProblemJson;
                        // Problème HTTP générique : le détail de l'exception reste dans les logs serveur.
                        await p_Context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                            title = "Erreur interne du serveur",
                            status = (int)HttpStatusCode.InternalServerError,
                            code = DomainErrors.UnknowError
                        }));
                    }
                }
                catch (Exception v_HandleErrorException)
                {
                    m_Logger.LogCritical(v_HandleErrorException,
                        "An error occurred while trying to print error in HandleExceptionMiddleware");
                }
            }
        }
    }
}