using AmetisAPI.Domain.Bo.Gtfs;

namespace AmetisAPI.Domain.Interfaces.Gtfs
{
    public interface IGtfsParser
    {
        /// <summary>Lit une archive GTFS. Lève <see cref="Exceptions.GtfsFormatException"/> si la structure est invalide.</summary>
        GtfsFeed Parse(Stream p_ZipStream);
    }
}
