namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// Presenter meant to be used to return content as JSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AResponseMessageJsonPresenter<T> : AJsonPresenter<T>
        where T : UseCaseResponseMessage
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected AResponseMessageJsonPresenter()
        {
        }

        /// <summary>
        /// Handle the response convertion to JSON
        /// </summary>
        /// <param name="p_Response"></param>
        public override void Handle(T p_Response)
        {
            ContentResult.StatusCode = (int)GetStatusCode(p_Response);
            ContentResult.Content = SerializationExtensions.PrettySerialize(p_Response.Success ? GetSuccessMember(p_Response) : p_Response.Errors);
        }

        /// <summary>
        /// Logic to determine status code for the response
        /// </summary>
        /// <param name="p_Response"></param>
        /// <returns></returns>
        protected override HttpStatusCode GetStatusCode(T p_Response)
        {
            // Customize here status code based on errors

            //Case where the data cannot be found
            if (p_Response.Errors?.Any(p_Obj => p_Obj.Code == DomainErrors.Data.NotFound) ?? false)
            {
                return HttpStatusCode.NotFound;
            }
            //Case where the data cannot be accessed
            if (p_Response.Errors?.Any(p_Obj => p_Obj.Code == DomainErrors.Data.Unauthorized) ?? false)
            {
                return HttpStatusCode.Unauthorized;
            }
            //Case where the request is invalid
            if (p_Response.Errors?.Any(p_Obj => p_Obj.Code == DomainErrors.Data.BadRequest) ?? false)
            {
                return HttpStatusCode.BadRequest;
            }
            //Case where an optional service is not configured
            if (p_Response.Errors?.Any(p_Obj => p_Obj.Code == DomainErrors.Push.Disabled) ?? false)
            {
                return HttpStatusCode.ServiceUnavailable;
            }
            return p_Response.Errors?.Any() != true ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
        }
    }
}