using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.Core.Requests.Crud
{
    public record EntityListRequest<T>() : IQuery<EntityListResponse<T>>
        where T : class, IBoEntity;
}