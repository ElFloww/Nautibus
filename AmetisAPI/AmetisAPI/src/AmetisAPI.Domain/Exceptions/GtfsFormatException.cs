namespace AmetisAPI.Domain.Exceptions
{
    /// <summary>Fichier GTFS structurellement invalide : l'import échoue sans toucher aux données actives.</summary>
    public class GtfsFormatException(string p_Message) : Exception(p_Message);
}
