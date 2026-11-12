using AmetisAPI.Core.Network;
using AmetisAPI.Core.Requests.Network;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.UseCases.Network
{
    /// <summary>Prochains passages théoriques d'un arrêt.</summary>
    internal class GetDeparturesUseCase(ILogger<GetDeparturesUseCase> p_Logger, IUnitOfWork p_UnitOfWork, INetworkRepository p_Network, IClock p_Clock)
        : ADbUseCase<GetDeparturesRequest, ValueResponse<IReadOnlyList<Departure>>>(p_Logger, p_UnitOfWork)
    {
        protected override Task<ValueResponse<IReadOnlyList<Departure>>> HandleLogic(GetDeparturesRequest p_Request, CancellationToken p_CancellationToken)
        {
            return WithDatabaseAsync(async () =>
            {
                StopDetail v_Stop = await p_Network.GetStopAsync(p_Request.StopId, p_CancellationToken);
                if (v_Stop is null)
                    return ValueResponse<IReadOnlyList<Departure>>.Error(DomainErrors.Data.NotFound, "Arrêt non trouvé");
                List<Departure> v_Departures = await DepartureCalculator.ComputeAsync(p_Network, v_Stop, p_Request.From ?? NetworkTime.ToParis(p_Clock.UtcNow), p_Request.WindowMinutes, p_CancellationToken);
                return new ValueResponse<IReadOnlyList<Departure>>(v_Departures.Take(p_Request.Limit).ToList());
            }, p_CancellationToken);
        }
    }
}
