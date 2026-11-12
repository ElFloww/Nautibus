using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.Domain.Tests.Bo
{
    public class DummyClass : IIdBoEntity
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }
}