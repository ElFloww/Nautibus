namespace AmetisAPI.Domain.Bo.Gtfs
{
    public record GtfsStop(string StopId, string Code, string Name, double Latitude, double Longitude, int LocationType, string ParentStationId, int WheelchairBoarding);
}
