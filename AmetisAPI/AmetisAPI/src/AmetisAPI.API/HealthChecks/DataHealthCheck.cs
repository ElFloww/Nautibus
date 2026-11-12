using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AmetisAPI.API.HealthChecks
{
    /// <summary>Expose l'état des données (imports, validité, temps réel) dans /health.</summary>
    internal class DataHealthCheck(IServiceScopeFactory p_ScopeFactory) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext p_Context, CancellationToken p_CancellationToken = default)
        {
            await using AsyncServiceScope v_Scope = p_ScopeFactory.CreateAsyncScope();
            ValueResponse<DataHealth> v_Result = await v_Scope.ServiceProvider.GetRequiredService<IMediator>()
                .SendQueryAsync<GetDataHealthRequest, ValueResponse<DataHealth>>(new GetDataHealthRequest(), p_CancellationToken);
            if (!v_Result.Success)
                return HealthCheckResult.Unhealthy("État des données indisponible");

            string v_Description = string.Join(" | ", v_Result.Value.Messages);
            return v_Result.Value.Status switch
            {
                DataHealthStatus.Healthy => HealthCheckResult.Healthy(v_Description),
                DataHealthStatus.Degraded => HealthCheckResult.Degraded(v_Description),
                _ => HealthCheckResult.Unhealthy(v_Description)
            };
        }
    }
}
