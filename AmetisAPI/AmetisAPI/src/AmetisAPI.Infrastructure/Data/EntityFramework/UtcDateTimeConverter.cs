using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AmetisAPI.Infrastructure.Data.EntityFramework
{
    /// <summary>Relit les dates SQL Server comme des dates UTC.</summary>
    internal class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(p_V => p_V, p_V => DateTime.SpecifyKind(p_V, DateTimeKind.Utc));
}
