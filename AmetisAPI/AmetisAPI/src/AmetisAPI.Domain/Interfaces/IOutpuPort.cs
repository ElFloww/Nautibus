namespace AmetisAPI.Domain.Interfaces
{
    public interface IOutputPort<in TUseCaseResponse>
    {
        void Handle(TUseCaseResponse p_Response);
    }
}