namespace AmetisAPI.Domain.Bo.Push
{
    public enum PushResult
    {
        Sent = 1,

        /// <summary>Abonnement expiré ou révoqué par le navigateur : à supprimer.</summary>
        Gone = 2,
        Failed = 3
    }
}
