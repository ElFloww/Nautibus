using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmetisAPI.Core.Responses;
using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo;
using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.Core.Requests.Crud
{
    public record EntityDeleteRequest<T>(List<int> EntitiesId) : IQuery<DeleteListResponse>
        where T : class, IBoEntity;
}