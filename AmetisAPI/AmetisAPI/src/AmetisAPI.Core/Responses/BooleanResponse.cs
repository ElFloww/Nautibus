namespace AmetisAPI.Core.Responses
{
    public class BooleanResponse : UseCaseResponseMessage<bool>
    {
        public BooleanResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public BooleanResponse(bool p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}