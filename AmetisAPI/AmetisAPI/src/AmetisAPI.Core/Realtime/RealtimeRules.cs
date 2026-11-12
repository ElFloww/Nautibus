namespace AmetisAPI.Core.Realtime
{
    /// <summary>Seuils de fraîcheur (audit P0 : flux rafraîchi toutes les 40 à 60 s).</summary>
    internal static class RealtimeRules
    {
        /// <summary>Au-delà, une donnée est affichée comme obsolète.</summary>
        public const int StaleAfterSeconds = 120;

        /// <summary>Au-delà, un véhicule n'est plus affiché du tout.</summary>
        public const int HideAfterSeconds = 600;

        /// <summary>Un passage temps réel déjà passé depuis plus longtemps n'est plus affiché.</summary>
        public const int PastArrivalToleranceSeconds = 60;

        public const string TripUpdatesProvenance = "ametis-trip-updates";

        /// <summary>Durée de rétention de l'historique des positions (P8.1).</summary>
        public const int HistoryRetentionDays = 30;

        /// <summary>Un arrêt est desservi dès que son heure effective publiée est dépassée.</summary>
        public static bool IsPassed(DateTime p_ExpectedUtc, DateTime p_NowUtc) => p_ExpectedUtc < p_NowUtc;

        public static int AgeSeconds(DateTime p_NowUtc, DateTime p_TimestampUtc) => (int)Math.Max(0, (p_NowUtc - p_TimestampUtc).TotalSeconds);

        public static Domain.Bo.Realtime.MatchConfidence Min(Domain.Bo.Realtime.MatchConfidence p_A, Domain.Bo.Realtime.MatchConfidence p_B) => p_A < p_B ? p_A : p_B;
    }
}
