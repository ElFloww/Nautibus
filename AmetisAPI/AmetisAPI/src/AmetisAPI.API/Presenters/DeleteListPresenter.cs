using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmetisAPI.API.Presenters
{
    /// <summary>
    ///
    /// </summary>
    public class DeleteListPresenter : AResponseMessageJsonPresenter<DeleteListResponse>
    {
        /// <summary>
        /// Success value
        /// </summary>
        /// <param name="p_Response"></param>
        /// <returns></returns>
        protected override object GetSuccessMember(DeleteListResponse p_Response)
        {
            return p_Response.Value;
        }
    }
}