namespace AmetisAPI.Domain.Interfaces.Network
{
    public interface INetworkShapeRepository
    {
        /// <summary>Tracés de chaque ligne du réseau actif : ligne → tracés → points [longitude, latitude].</summary>
        Task<IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<double[]>>>> GetShapesByLineAsync(CancellationToken p_CancellationToken);
    }
}
