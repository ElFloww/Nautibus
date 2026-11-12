using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmetisAPI.Core.Responses;
using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.Core.Responses.Crud
{
    public class EntityResponse<T> : UseCaseResponseMessage<T> where T : class, IBoEntity
    {
        public EntityResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public EntityResponse(T p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}