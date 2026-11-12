using Mapster;

namespace AmetisAPI.Infrastructure.Data.EntityFramework.Mapping
{
    internal class EntityFrameworkProfile : IRegister
    {

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Inherit name")]
        public void Register(TypeAdapterConfig config)
        {
            //config.NewConfig<Obj, Obj2>()
            //    .Map(p_Dest => p_Dest.Id, p_Src => p_Src.Id);
        }
    }
}