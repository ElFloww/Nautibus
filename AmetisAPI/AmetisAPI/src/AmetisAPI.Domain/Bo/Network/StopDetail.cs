namespace AmetisAPI.Domain.Bo.Network
{
    public record StopDetail(StopSummary Stop, IReadOnlyList<StopSummary> Quays);
}
