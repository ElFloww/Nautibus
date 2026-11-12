using AmetisAPI.Domain.Bo;

namespace AmetisAPI.Core.Responses
{
    public class FileResponse : UseCaseResponseMessage<DocumentFile>
    {
        public FileResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public FileResponse(DocumentFile p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}