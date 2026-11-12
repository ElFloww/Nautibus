namespace AmetisAPI.Domain.Bo.Crud
{
    public interface IBoEntity;

    public interface IIdBoEntity : IBoEntity
    {
        public int? Id { get; set; }
    }
}