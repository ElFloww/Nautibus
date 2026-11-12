using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmetisAPI.Core.Responses;
using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.Core.Responses.Crud
{
    public class EntityListResponse<T> : UseCaseResponseMessage<IEnumerable<T>>
        where T : class, IBoEntity
    {
        public EntityListResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public EntityListResponse(IEnumerable<T> p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}