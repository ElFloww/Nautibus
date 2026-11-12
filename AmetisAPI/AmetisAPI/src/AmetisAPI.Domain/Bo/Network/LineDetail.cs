namespace AmetisAPI.Domain.Bo.Network
{
    public record LineDetail(LineSummary Line, IReadOnlyList<LineDirection> Directions);
}
