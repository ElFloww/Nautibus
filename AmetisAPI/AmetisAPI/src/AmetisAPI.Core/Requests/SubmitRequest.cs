using AmetisAPI.Core.Responses;

namespace AmetisAPI.Core.Requests
{
    public class SubmitRequest : IQuery<UseCaseResponseMessage>
    {
        public string Name { get; set; }
    }
}
