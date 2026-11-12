namespace AmetisAPI.Domain.Bo.Network
{
    /// <summary>Tracé : coordonnées [longitude, latitude] dans l'ordre GeoJSON.</summary>
    public record LineShape(string ShapeId, IReadOnlyList<double[]> Coordinates);
}
