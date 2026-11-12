namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>Les deux GTFS publiés par Ametis (voir docs/P0-audit-donnees.md).</summary>
    public enum FeedSource
    {
        /// <summary>GTFS complet : référence réseau (tracés, couleurs, TAD, scolaires).</summary>
        Network = 1,

        /// <summary>GTFS « pour le GTFS-RT » : identifiants du temps réel.</summary>
        Realtime = 2
    }
}
