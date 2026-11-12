namespace AmetisAPI.Domain.Bo.Gtfs
{
    /// <summary>Enregistrement écarté à l'import, avec sa raison (jamais supprimé en silence).</summary>
    public record GtfsRejection(string FileName, int LineNumber, string Reason);
}
