using AmetisAPI.Core.Responses;
using AmetisAPI.Domain.Errors;

namespace AmetisAPI.Core.Extensions
{
    public static class ErrorExtensions
    {
        public static ErrorDto ToError(this Exception p_Ex)
        {
            return new ErrorDto(DomainErrors.UnknowError, p_Ex.Message, p_Ex);
        }

        public static ErrorDto ToError(this Exception p_Ex, string p_ErrorCode)
        {
            return new ErrorDto(p_ErrorCode, p_Ex.Message, p_Ex);
        }

        public static IEnumerable<ErrorDto> ToError(this AggregateException p_Ex, string p_ErrorCode = null)
        {
            return new List<ErrorDto>(p_Ex.InnerExceptions.Select(p_InnerEx => p_InnerEx.ToError(p_ErrorCode)));
        }
    }
}