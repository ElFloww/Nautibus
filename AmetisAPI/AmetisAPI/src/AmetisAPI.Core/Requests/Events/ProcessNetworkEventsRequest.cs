namespace AmetisAPI.Core.Requests.Events
{
    /// <summary>Après chaque collecte : déviations, évènements et notifications push. Valeur : nombre d'évènements créés.</summary>
    public record ProcessNetworkEventsRequest : IQuery<ValueResponse<int>>;
}
