using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Domain.Bo.Crud;

namespace AmetisAPI.API.Presenters.Crud
{
    /// <summary>
    /// Get the entity value
    /// </summary>
    public class GenericEntityPresenter<T> : AResponseMessageJsonPresenter<EntityResponse<T>>
        where T : class, IBoEntity
    {
        /// <summary>
        /// Success value
        /// </summary>
        /// <param name="p_Response"></param>
        /// <returns></returns>
        protected override object GetSuccessMember(EntityResponse<T> p_Response)
        {
            return p_Response.Value;
        }
    }
}