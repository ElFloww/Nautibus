using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Mapping
{
    internal class MappingAddEntity<T, TT> : AMappingClass, IMappingAddEntity<T, TT>
        where T : class, IBoEntity
        where TT : class, IEntity
    {
        public MappingAddEntity(IMapper p_Mapper, T p_BoEntity) : this(p_Mapper, p_BoEntity, p_Mapper.Map<TT>(p_BoEntity))
        { }

        public MappingAddEntity(IMapper p_Mapper, T p_BoEntity, TT p_DtoEntity) : base(p_Mapper)
        {
            BoEntity = p_BoEntity ?? throw new ArgumentNullException(nameof(p_BoEntity));
            DtoEntity = p_DtoEntity ?? throw new ArgumentNullException(nameof(p_DtoEntity));
        }

        public T BoEntity { get; }

        public TT DtoEntity { get; }

        public T MapBoEntity => Mapper.Map(DtoEntity, BoEntity);

        public TT MapDtoEntity => Mapper.Map(BoEntity, DtoEntity);
    }
}