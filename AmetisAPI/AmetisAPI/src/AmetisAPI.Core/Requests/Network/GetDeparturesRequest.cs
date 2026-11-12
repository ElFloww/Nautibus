using AmetisAPI.Domain.Bo.Network;

namespace AmetisAPI.Core.Requests.Network
{
    /// <summary>Prochains passages théoriques à partir de <paramref name="From"/> (maintenant si absent).</summary>
    public record GetDeparturesRequest(string StopId, DateTimeOffset? From = null, int Limit = 20, int WindowMinutes = 180) : IQuery<ValueResponse<IReadOnlyList<Departure>>>;
}
