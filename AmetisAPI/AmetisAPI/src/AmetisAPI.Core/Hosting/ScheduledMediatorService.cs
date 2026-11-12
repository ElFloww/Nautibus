using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AmetisAPI.Core.Hosting
{
    /// <summary>
    /// Exécute périodiquement une requête du médiateur dans sa propre portée DI. Une erreur est journalisée
    /// et n'arrête pas la boucle (redémarrage implicite au tour suivant).
    /// </summary>
    public abstract class ScheduledMediatorService(IServiceScopeFactory p_ScopeFactory, ILogger p_Logger) : BackgroundService
    {
        protected readonly ILogger m_Logger = p_Logger;

        protected abstract TimeSpan Interval { get; }

        protected abstract Task RunOnceAsync(IMediator p_Mediator, CancellationToken p_CancellationToken);

        protected override async Task ExecuteAsync(CancellationToken p_StoppingToken)
        {
            using PeriodicTimer v_Timer = new(Interval);
            do
            {
                try
                {
                    await using AsyncServiceScope v_Scope = p_ScopeFactory.CreateAsyncScope();
                    await RunOnceAsync(v_Scope.ServiceProvider.GetRequiredService<IMediator>(), p_StoppingToken);
                }
                catch (Exception v_Ex) when (v_Ex is not OperationCanceledException)
                {
                    m_Logger.LogError(v_Ex, "Tâche planifiée {Service} en échec", GetType().Name);
                }
            }
            while (await v_Timer.WaitForNextTickAsync(p_StoppingToken));
        }
    }
}
