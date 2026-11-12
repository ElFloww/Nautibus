namespace AmetisAPI.Domain.Bo.Realtime
{
    /// <summary>
    /// Passage d'une course à un arrêt, tel que publié dans les Trip Updates Ametis : heure prévue (heure publiée moins retard)
    /// et heure effective publiée (réalisée pour un arrêt déjà desservi, estimée pour un arrêt à venir).
    /// StopOrder : rang de l'arrêt dans la course. IsLastStop : terminus de la course (calculé à la lecture).
    /// </summary>
    public record StopPassage(string TripId, string RouteId, string StopId, int StopOrder, DateTime ScheduledUtc, DateTime ExpectedUtc, int DelaySeconds, string VehicleId, bool IsLastStop = false);
}
