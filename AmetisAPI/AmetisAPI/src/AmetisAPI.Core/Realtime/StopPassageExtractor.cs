using AmetisAPI.Domain.Bo.Realtime;

namespace AmetisAPI.Core.Realtime
{
    /// <summary>
    /// Passages à enregistrer depuis les Trip Updates : le flux Ametis liste tous les arrêts de chaque course dans l'ordre,
    /// sans numéro de séquence ; le rang est donc l'ordre de publication. L'heure prévue est l'heure publiée moins le retard
    /// (vérifié à la seconde près contre le GTFS lors de l'analyse). Sans retard publié, l'heure prévue est inconnue : non enregistré.
    /// </summary>
    internal static class StopPassageExtractor
    {
        public static IReadOnlyList<StopPassage> Extract(IReadOnlyList<StopTimePrediction> p_Predictions)
        {
            List<StopPassage> v_Passages = [];
            foreach (IGrouping<string, StopTimePrediction> v_Trip in p_Predictions.GroupBy(p_P => p_P.TripId))
            {
                int v_Order = 0;
                foreach (StopTimePrediction v_Prediction in v_Trip)
                {
                    v_Order++;
                    if (v_Prediction.StopId is null || v_Prediction.DelaySeconds is not { } v_Delay || (v_Prediction.DepartureUtc ?? v_Prediction.ArrivalUtc) is not { } v_Expected)
                        continue;
                    v_Passages.Add(new StopPassage(v_Trip.Key, v_Prediction.RouteId, v_Prediction.StopId, v_Prediction.StopSequence ?? v_Order,
                        v_Expected.AddSeconds(-v_Delay), v_Expected, v_Delay, v_Prediction.VehicleId));
                }
            }
            return v_Passages;
        }
    }
}
