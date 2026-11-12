namespace AmetisAPI.Domain.Bo.Gtfs
{
    public enum FeedImportStatus
    {
        Running = 1,
        Succeeded = 2,
        Failed = 3,

        /// <summary>Contenu identique à la version active : rien n'est réimporté.</summary>
        Skipped = 4
    }
}
