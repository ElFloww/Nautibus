using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Entities
{
    public record FakeIIdEntity : IIdEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
