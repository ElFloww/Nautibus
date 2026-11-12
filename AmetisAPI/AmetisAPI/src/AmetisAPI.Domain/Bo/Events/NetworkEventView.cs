using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Domain.Bo.Events
{
    public record NetworkEventView(
        int Id,
        NetworkEventKind Kind,
        string Title,
        string Description,
        string Url,
        IReadOnlyList<LineSummary> Lines,
        DateTime StartUtc,
        DateTime? EndUtc,
        DateTime? ClosedUtc,
        DateTime LastSeenUtc,
        MatchConfidence Confidence,
        bool IsActive);
}
