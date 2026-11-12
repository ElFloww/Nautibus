namespace AmetisAPI.Domain.Bo.Realtime
{
    public record ServiceAlert(string Id, string Header, string Description, string Url, DateTime? StartUtc, DateTime? EndUtc, IReadOnlyList<string> RouteIds, IReadOnlyList<string> StopIds);
}
