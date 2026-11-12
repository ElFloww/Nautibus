using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Bo.Events
{
    /// <summary>Évènement constaté lors d'une collecte, avant rapprochement avec ceux déjà enregistrés.</summary>
    public record NetworkEventDraft(string ExternalId, string Title, string Description, string Url, IReadOnlyList<string> LineIds, DateTime? StartUtc, DateTime? EndUtc, MatchConfidence Confidence);
}
