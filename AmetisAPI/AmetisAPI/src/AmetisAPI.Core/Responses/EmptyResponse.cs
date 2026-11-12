namespace AmetisAPI.Core.Responses
{
    public class EmptyResponse : UseCaseResponseMessage
    {
        public EmptyResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null)
            : base(p_Errors, p_Message)
        {
        }

        public EmptyResponse(string p_Message = null)
            : base(p_Message)
        {
        }
    }
}