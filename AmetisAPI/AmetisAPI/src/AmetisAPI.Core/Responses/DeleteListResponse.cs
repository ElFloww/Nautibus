using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmetisAPI.Core.Responses
{
    public class DeleteListResponse : UseCaseResponseMessage<Dictionary<int, bool>>
    {
        public DeleteListResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public DeleteListResponse(Dictionary<int, bool> p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}