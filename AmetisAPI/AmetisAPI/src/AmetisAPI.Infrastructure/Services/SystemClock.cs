using AmetisAPI.Domain.Interfaces;

namespace AmetisAPI.Infrastructure.Services
{
    internal class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
