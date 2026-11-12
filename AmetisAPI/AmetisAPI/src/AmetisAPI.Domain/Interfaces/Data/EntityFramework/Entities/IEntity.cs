namespace AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities
{
    public interface IEntity
    {
    }

    public interface IIdEntity : IEntity
    {
        public int Id { get; set; }
    }
}