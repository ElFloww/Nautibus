namespace AmetisAPI.Domain.Bo.Gtfs
{
    public record GtfsRoute(string RouteId, string ShortName, string LongName, int RouteType, string Color, string TextColor, int? SortOrder);
}
