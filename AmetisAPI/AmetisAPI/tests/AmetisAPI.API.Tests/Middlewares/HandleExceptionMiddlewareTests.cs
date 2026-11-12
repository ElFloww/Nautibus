using AmetisAPI.API.Middlewares;
using Microsoft.AspNetCore.Http;
using AmetisAPI.Domain.Errors;
using Microsoft.Extensions.Logging.Abstractions;

namespace AmetisAPI.API.Tests.Middlewares
{
    public class HandleExceptionMiddlewareTests
    {
        [Fact]
        public async Task Unhandled_Exception_Returns_Generic_Problem_Without_Internal_Details()
        {
            HandleExceptionMiddleware v_Middleware = new(_ => throw new InvalidOperationException("détail interne secret"), NullLogger<HandleExceptionMiddleware>.Instance);
            DefaultHttpContext v_Context = new();
            v_Context.Response.Body = new MemoryStream();

            await v_Middleware.InvokeAsync(v_Context);

            v_Context.Response.Body.Position = 0;
            string v_Body = await new StreamReader(v_Context.Response.Body).ReadToEndAsync();
            Check.That(v_Context.Response.StatusCode).IsEqualTo(500);
            Check.That(v_Context.Response.ContentType).IsEqualTo("application/problem+json");
            Check.That(v_Body).Not.Contains("secret").And.Not.Contains("StackTrace").And.Not.Contains("InvalidOperationException");
            Check.That(v_Body).Contains(DomainErrors.UnknowError);
        }
    }
}
