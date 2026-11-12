using System.Net.Mime;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace AmetisAPI.API.Controllers
{
    /// <summary>
    /// Base controller used for API controllers
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]s")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger m_Logger;

        /// <summary>
        /// Mediator Service
        /// </summary>
        protected readonly IMediator m_Mediator;

        /// <summary>
        /// ValidationPresenter used to return validation errors if any
        /// </summary>
        protected readonly ValidationPresenter m_ValidationPresenter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="p_Logger"></param>
        /// <param name="p_Mediator"></param>
        /// <param name="p_ValidationPresenter"></param>
        protected BaseController(ILogger p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
        {
            m_Logger = p_Logger;
            m_Mediator = p_Mediator;
            m_ValidationPresenter = p_ValidationPresenter;
        }

        /// <summary>
        /// Valide la requête (si un validateur existe), l'envoie au médiateur et présente la réponse.
        /// </summary>
        protected async Task<IActionResult> SendAsync<TRequest, TValue>(TRequest p_Request, CancellationToken p_CancellationToken)
            where TRequest : Cortex.Mediator.Queries.IQuery<ValueResponse<TValue>>
        {
            if (HttpContext?.RequestServices.GetService<IValidator<TRequest>>() is { } v_Validator)
            {
                ValidationResult v_Validation = await v_Validator.ValidateAsync(p_Request, p_CancellationToken);
                if (!v_Validation.IsValid)
                {
                    m_ValidationPresenter.Handle(v_Validation);
                    return m_ValidationPresenter.ContentResult;
                }
            }

            ValueResponse<TValue> v_Response = await m_Mediator.SendQueryAsync<TRequest, ValueResponse<TValue>>(p_Request, p_CancellationToken);
            ValuePresenter<TValue> v_Presenter = new();
            v_Presenter.Handle(v_Response);
            return v_Presenter.ContentResult;
        }
    }
}
