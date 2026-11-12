using System.Diagnostics.CodeAnalysis;
using AmetisAPI.Core.Responses;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.UseCases
{
    internal abstract class ADbUseCase<TRequest, TResponse>(ILogger p_Logger, IUnitOfWork p_UnitOfWork)
        : AUseCase<TRequest, TResponse>(p_Logger)
        where TRequest : IQuery<TResponse>
    {
        protected readonly IUnitOfWork m_UnitOfWork = p_UnitOfWork;
        protected static readonly SemaphoreSlim m_DatabaseProtection = new(1, 1);

        /// <summary>Exécute un accès base sous la protection du sémaphore du cas d'usage.</summary>
        protected static async Task<T> WithDatabaseAsync<T>(Func<Task<T>> p_Action, CancellationToken p_CancellationToken)
        {
            await m_DatabaseProtection.WaitAsync(p_CancellationToken);
            try
            {
                return await p_Action();
            }
            finally
            {
                m_DatabaseProtection.Release();
            }
        }
    }

    internal abstract class AUseCase<TRequest, TResponse>(ILogger p_Logger) : IQueryHandler<TRequest, TResponse>
        where TRequest : IQuery<TResponse>
    {
        protected readonly ILogger m_Logger = p_Logger;

        [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await HandleLogic(request, cancellationToken);
            }
            catch (Exception ex)
            {
                return await HandleException(ex, request, cancellationToken);
            }
        }

        protected abstract Task<TResponse> HandleLogic(TRequest p_Request, CancellationToken p_CancellationToken);

        protected virtual Task<TResponse> HandleException(Exception p_Exception, TRequest p_Request,
            CancellationToken p_CancellationToken)
        {
            const string v_ErrorDescription = "An unknown error occurred";

            m_Logger.LogError(p_Exception, v_ErrorDescription);

            return Task.FromResult((TResponse)Activator.CreateInstance(
                typeof(TResponse),
                new List<ErrorDto> { new(DomainErrors.UnknowError, v_ErrorDescription, p_Exception) },
                v_ErrorDescription
            ));
        }
    }
}