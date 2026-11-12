using AmetisAPI.Domain.Bo.Push;

namespace AmetisAPI.Domain.Interfaces.Push
{
    public interface IPushSender
    {
        /// <summary>False si les clés VAPID ne sont pas configurées.</summary>
        bool IsEnabled { get; }

        /// <summary>Clé publique VAPID (base64url) transmise aux navigateurs.</summary>
        string PublicKey { get; }

        Task<PushResult> SendAsync(PushSubscriptionInfo p_Subscription, PushMessage p_Message, CancellationToken p_CancellationToken);
    }
}
