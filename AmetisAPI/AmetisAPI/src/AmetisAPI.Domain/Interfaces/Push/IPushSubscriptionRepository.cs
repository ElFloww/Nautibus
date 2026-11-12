using AmetisAPI.Domain.Bo.Push;

namespace AmetisAPI.Domain.Interfaces.Push
{
    /// <summary>Abonnements push des utilisateurs (données personnelles, supprimées avec le compte).</summary>
    public interface IPushSubscriptionRepository
    {
        /// <summary>Crée ou met à jour l'abonnement de ce navigateur pour l'utilisateur.</summary>
        Task SaveAsync(string p_UserId, PushSubscriptionInfo p_Subscription, bool p_AllLines, DateTime p_NowUtc, CancellationToken p_CancellationToken);

        Task<bool> RemoveAsync(string p_UserId, string p_Endpoint, CancellationToken p_CancellationToken);

        /// <summary>Abonnements concernés : « toutes les lignes », ou ligne parmi les favoris de l'utilisateur.</summary>
        Task<IReadOnlyList<PushSubscriptionInfo>> ListForLinesAsync(IReadOnlyCollection<string> p_LineIds, CancellationToken p_CancellationToken);

        Task RemoveByEndpointAsync(string p_Endpoint, CancellationToken p_CancellationToken);
    }
}
