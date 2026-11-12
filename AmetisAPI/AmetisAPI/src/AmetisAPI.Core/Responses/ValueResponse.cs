namespace AmetisAPI.Core.Responses
{
    /// <summary>Réponse générique d'un cas d'usage : une valeur ou des erreurs.</summary>
    public class ValueResponse<T> : UseCaseResponseMessage<T>
    {
        public ValueResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public ValueResponse(T p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }

        public static ValueResponse<T> Error(string p_Code, string p_Description) => new([new ErrorDto(p_Code, p_Description)]);
    }
}
