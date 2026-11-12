using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmetisAPI.Core.Responses
{
    public class IntResponse : UseCaseResponseMessage<int>
    {
        public IntResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public IntResponse(int p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}