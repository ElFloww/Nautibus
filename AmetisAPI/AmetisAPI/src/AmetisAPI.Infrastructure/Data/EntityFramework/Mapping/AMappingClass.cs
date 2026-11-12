using MapsterMapper;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Mapping
{
    internal class AMappingClass
    {
        protected AMappingClass(IMapper p_Mapper)
        {
            Mapper = Guard.Against.Null(p_Mapper, nameof(p_Mapper), "Mapper cannot be null");
        }

        public IMapper Mapper { get; }
    }
}