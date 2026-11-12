using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.API.Presenters.Crud
{
    /// <summary>
    /// Get the entity list value
    /// </summary>
    public class GenericEntityListPresenter<T> : AResponseMessageJsonPresenter<EntityListResponse<T>>
        where T : class, IBoEntity
    {
        /// <summary>
        /// Success value
        /// </summary>
        /// <param name="p_Response"></param>
        /// <returns></returns>
        protected override object GetSuccessMember(EntityListResponse<T> p_Response)
        {
            return p_Response.Value;
        }
    }
}