namespace AmetisAPI.Domain.Interfaces
{
    /// <summary>Horloge injectée (tests déterministes).</summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
