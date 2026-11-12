using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Bo.Events
{
    /// <summary>
    /// Évènement enregistré. EndUtc : fin annoncée ; ClosedUtc : date à laquelle il a disparu du flux ou n'est plus détecté.
    /// </summary>
    public record NetworkEvent(
        int Id,
        NetworkEventKind Kind,
        string ExternalId,
        string Title,
        string Description,
        string Url,
        IReadOnlyList<string> LineIds,
        DateTime StartUtc,
        DateTime? EndUtc,
        DateTime? ClosedUtc,
        DateTime LastSeenUtc,
        MatchConfidence Confidence)
    {
        public bool IsActiveAt(DateTime p_NowUtc) => ClosedUtc is null && (EndUtc is null || EndUtc > p_NowUtc) && StartUtc <= p_NowUtc;
    }
}
