namespace AmetisAPI.Core.Responses
{
    public class StringResponse : UseCaseResponseMessage<string>
    {
        public StringResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public StringResponse(string p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}