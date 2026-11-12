using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Entities
{
    public record FakeIBoEntity : IBoEntity
    {
        public int? Id { get; set; }

        public string Name { get; set; }
    }

}
